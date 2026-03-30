import os
import re
import json
import zipfile
import threading
from io import BytesIO
from datetime import datetime
from typing import Dict, Any, List, Optional

import PluginUtils
from http_client import get_global_client
from steam_utils import detect_steam_install_path, get_stplug_in_path

logger = PluginUtils.Logger()


def _get_depotcache_path() -> str:
    """Returns the depotcache folder inside the Steam config directory."""
    steam_path = detect_steam_install_path()
    if not steam_path:
        raise RuntimeError("Steam installation path not found")
    depot_path = os.path.join(steam_path, 'depotcache')
    os.makedirs(depot_path, exist_ok=True)
    return depot_path


class LuaManager:
    def __init__(self, backend_path: str):
        self.backend_path = backend_path
        self._download_state: Dict[int, Dict[str, Any]] = {}
        self._download_lock = threading.Lock()

    # ------------------------------------------------------------------ state

    def _set_download_state(self, appid: int, update: Dict[str, Any]) -> None:
        with self._download_lock:
            state = self._download_state.get(appid, {})
            state.update(update)
            self._download_state[appid] = state

    def _get_download_state(self, appid: int) -> Dict[str, Any]:
        with self._download_lock:
            return self._download_state.get(appid, {}).copy()

    def get_download_status(self, appid: int) -> Dict[str, Any]:
        return {'success': True, 'state': self._get_download_state(appid)}

    # ----------------------------------------------------------- core download

    def _download_backend(self, appid: int) -> None:
        """
        Downloads the ManifestHub ZIP branch for `appid` from GitHub,
        extracts the first .lua file into stplug-in/{appid}.lua,
        and copies all .manifest files into config/depotcache/.
        """
        self._set_download_state(appid, {
        'status': 'checking',
        'currentApi': 'ManifestHub',
        'bytesRead': 0,
        'totalBytes': 0,
        'endpoint': 'Github'
        })

        api_base = "https://api.steamproof.net"
        github_url = f'https://codeload.github.com/SteamAutoCracks/ManifestHub/zip/refs/heads/{appid}'
        
        client = get_global_client()
        if not client:
            self._set_download_state(appid, {'status': 'failed', 'error': 'Failed to get HTTP client'})
            return

        try:
            self._set_download_state(appid, {'status': 'downloading', 'endpoint': 'ManifestHub'})
            status, headers, body = client.raw_get(github_url)

            if status != 200:
                self._set_download_state(appid, {'status': 'failed', 'error': f'ManifestHub Lua not found (HTTP {status})'})
                return

            lua_dir = get_stplug_in_path()
            depot_dir = _get_depotcache_path()
            dst_lua = os.path.join(lua_dir, f'{appid}.lua')

            with zipfile.ZipFile(BytesIO(body), 'r') as z:
                all_names = z.namelist()
                lua_files = [n for n in all_names if n.lower().endswith('.lua')]
                
                if not lua_files:
                    self._set_download_state(appid, {'status': 'failed', 'error': 'No .lua file in GitHub ZIP'})
                    return

                raw_data = z.read(lua_files[0]).decode('utf-8', errors='ignore')
                clean_content = re.sub(r'(?m)^\s*setManifestid\(.*?\);?\s*\n?', '', raw_data)
                clean_content = re.sub(r'(?s)\n-- SteamProof Manifests.*', '', clean_content).strip()

            self._set_download_state(appid, {'status': 'downloading', 'endpoint': 'SteamProof Fix'})
            
            st_info, _, body_info = client.raw_get(f"{api_base}/apps/depots?ids={appid}")
            st_dl, _, body_dl = client.raw_get(f"{api_base}/app/{appid}/manifests/download")

            if st_info != 200 or st_dl != 200:
                self._set_download_state(appid, {'status': 'failed', 'error': 'SteamProof API error'})
                return

            app_data = json.loads(body_info)['apps'][0]
            manifest_list = json.loads(body_dl)['manifests']
            
            installed_files = [dst_lua]
            manifest_lines = []
            
            for m in manifest_list:
                did = m['depotId']
                mid = m['manifestId']
                m_url = m['url']
                
                dst_manifest = os.path.join(depot_dir, f"{did}_{mid}.manifest")
                if not os.path.exists(dst_manifest):
                    m_status, _, m_body = client.raw_get(m_url)
                    if m_status == 200:
                        with open(dst_manifest, 'wb') as f:
                            f.write(m_body)
                        installed_files.append(dst_manifest)

                depot_info = next((d for d in app_data['depots'] if d['depotId'] == did), None)
                max_size = depot_info.get('maxSize') if depot_info else None
                
                if max_size:
                    manifest_lines.append(f'setManifestid({did}, "{mid}", {max_size})')
                else:
                    manifest_lines.append(f'setManifestid({did}, "{mid}")')

            timestamp = datetime.utcnow().strftime('%Y-%m-%d %H:%M UTC')
            final_lua_content = clean_content + f"\n\n-- SteamProof Manifests (updated {timestamp})\n"
            final_lua_content += "\n".join(manifest_lines) + "\n"

            with open(dst_lua, 'w', encoding='utf-8') as f:
                f.write(final_lua_content)

            self._set_download_state(appid, {
                'status': 'done',
                'success': True,
                'api': 'ManifestHub',
                'installedFiles': installed_files,
                'installedPath': dst_lua
            })

        except Exception as e:
            logger.error(f'LuaManager: Hybrid install failed for {appid}: {e}')
            self._set_download_state(appid, {'status': 'failed', 'error': str(e)})

    # ---------------------------------------------------------- public API

    def add_via_lua(self, appid: int, endpoints: Optional[List[str]] = None) -> Dict[str, Any]:
        try:
            appid = int(appid)
        except (ValueError, TypeError):
            return {'success': False, 'error': 'Invalid appid'}

        self._set_download_state(appid, {'status': 'queued', 'bytesRead': 0, 'totalBytes': 0})

        def run():
            try:
                self._download_backend(appid)
            except Exception as e:
                logger.error(f'LuaManager: unhandled error for {appid}: {e}')
                self._set_download_state(appid, {'status': 'failed', 'error': f'Crash: {str(e)}'})

        threading.Thread(target=run, daemon=True).start()
        return {'success': True}

    def remove_via_lua(self, appid: int) -> Dict[str, Any]:
        try:
            appid = int(appid)
        except (ValueError, TypeError):
            return {'success': False, 'error': 'Invalid appid'}

        try:
            stplug = get_stplug_in_path()
            removed = []

            lua_file = os.path.join(stplug, f'{appid}.lua')
            if os.path.exists(lua_file):
                os.remove(lua_file)
                removed.append(f'{appid}.lua')

            disabled = os.path.join(stplug, f'{appid}.lua.disabled')
            if os.path.exists(disabled):
                os.remove(disabled)
                removed.append(f'{appid}.lua.disabled')

            try:
                depot = _get_depotcache_path()
                for name in os.listdir(depot):
                    if name.startswith(f'{appid}_') and name.endswith('.manifest'):
                        os.remove(os.path.join(depot, name))
                        removed.append(name)
            except Exception as e:
                logger.log(f'LuaManager: Could not remove manifests for {appid}: {e}')

            if removed:
                logger.log(f'LuaManager: Removed {removed}')
                return {'success': True, 'message': f'Removed {len(removed)} files', 'removed_files': removed}
            return {'success': False, 'error': f'No files found for app {appid}'}
        except Exception as e:
            logger.error(f'LuaManager: remove error for {appid}: {e}')
            return {'success': False, 'error': str(e)}
