import os
import zipfile
import threading
from io import BytesIO
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
    depot_path = os.path.join(steam_path, 'config', 'depotcache')
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
            'currentApi': 'manifesthub',
            'bytesRead': 0,
            'totalBytes': 0,
            'endpoint': 'manifesthub'
        })

        download_url = f'https://codeload.github.com/SteamAutoCracks/ManifestHub/zip/refs/heads/{appid}'

        client = get_global_client()
        if not client:
            self._set_download_state(appid, {'status': 'failed', 'error': 'Failed to get HTTP client'})
            return

        try:
            self._set_download_state(appid, {'status': 'downloading', 'endpoint': 'manifesthub'})
            status, headers, body = client.raw_get(download_url)
        except Exception as e:
            logger.error(f'LuaManager: HTTP request failed for {appid}: {e}')
            self._set_download_state(appid, {'status': 'failed', 'error': f'Request error: {str(e)}'})
            return

        if status == 404:
            self._set_download_state(appid, {'status': 'failed', 'error': f'App {appid} not found on ManifestHub'})
            return
        if status < 200 or status >= 300:
            self._set_download_state(appid, {'status': 'failed', 'error': f'HTTP {status}'})
            return
        if not isinstance(body, (bytes, bytearray)) or len(body) == 0:
            self._set_download_state(appid, {'status': 'failed', 'error': 'Empty response from ManifestHub'})
            return

        clen = headers.get('Content-Length', '')
        try:
            clen_int = int(clen) if clen and str(clen).isdigit() else len(body)
        except Exception:
            clen_int = len(body)

        logger.log(f'LuaManager: Downloaded {len(body)} bytes for appid {appid}')
        self._set_download_state(appid, {'status': 'processing', 'bytesRead': len(body), 'totalBytes': clen_int})

        try:
            lua_dir = get_stplug_in_path()
            depot_dir = _get_depotcache_path()

            installed_files = []

            with zipfile.ZipFile(BytesIO(body), 'r') as z:
                all_names = z.namelist()

                lua_files = [n for n in all_names if n.lower().endswith('.lua')]
                manifest_files = [n for n in all_names if n.lower().endswith('.manifest')]

                if lua_files:
                    dst_lua = os.path.join(lua_dir, f'{appid}.lua')
                    data = z.read(lua_files[0])
                    with open(dst_lua, 'wb') as f:
                        f.write(data)
                    installed_files.append(dst_lua)
                    logger.log(f'LuaManager: Installed Lua -> {dst_lua}')
                else:
                    logger.log(f'LuaManager: No .lua file found in ZIP for appid {appid}')

                for mname in manifest_files:
                    filename = os.path.basename(mname)
                    if not filename:
                        continue
                    dst_manifest = os.path.join(depot_dir, filename)
                    data = z.read(mname)
                    with open(dst_manifest, 'wb') as f:
                        f.write(data)
                    installed_files.append(dst_manifest)
                    logger.log(f'LuaManager: Installed Manifest -> {dst_manifest}')

            if not installed_files:
                self._set_download_state(appid, {'status': 'failed', 'error': 'No .lua or .manifest files found in ZIP'})
                return

            self._set_download_state(appid, {
                'status': 'done',
                'success': True,
                'api': 'manifesthub',
                'installedFiles': installed_files,
                'installedPath': installed_files[0] if installed_files else ''
            })

        except zipfile.BadZipFile:
            logger.error(f'LuaManager: Bad ZIP received for appid {appid}')
            self._set_download_state(appid, {'status': 'failed', 'error': 'Invalid ZIP file received from ManifestHub'})
        except Exception as e:
            logger.error(f'LuaManager: install failed for {appid}: {e}')
            self._set_download_state(appid, {'status': 'failed', 'error': f'Install failed: {str(e)}'})

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
