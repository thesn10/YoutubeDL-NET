# temporary imports, going to be replaced by a c# version
#import os
#import io

#from .youtube_dl.cache import Cache
#from .youtube_dl.utils import (
    #sanitized_Request,
    #YoutubeDLCookieJar,
    #YoutubeDLCookieProcessor,
    #YoutubeDLHandler,
    #PerRequestProxyHandler,
    #make_HTTPS_handler,
    #make_socks_conn_class,
    #_create_http_connection,
#)
#from .youtube_dl.compat import (
    #compat_str,
    #compat_basestring,
    #compat_urllib_request,
    #compat_urllib_request_DataHandler,
    #compat_urllib_error,
    #compat_cookiejar,
    #compat_cookies,
    #compat_expanduser,
    #compat_urlparse,
    #compat_http_client,
#)


import urllib.request as compat_urllib_request
import urllib.error as compat_urllib_error
import http.cookiejar as compat_cookiejar
import http.cookies as compat_cookies

class FakeYTDL(object):

    def __init__(self, ytdl):
        self.cs_ytdl = ytdl
        self.params = self.cs_ytdl.GetOptions()
        #self.cache = Cache(self)
        self.cache = FakeCache(ytdl)
        self.cookiejar = FakeCookieJar(ytdl)
        self._setup_opener()
        return

    def oldurlopen(self, req):
        """ Start an HTTP download """
        # temporary solution, going to be replaced by a c# version
        #if isinstance(req, compat_basestring):
        #    req = sanitized_Request(req)
        #return self._opener.open(req, timeout=self._socket_timeout)

    def urlopen(self, req):
        """ Start an HTTP download """
        if isinstance(req, str):
            req = compat_urllib_request.Request(req)
        #self.cs_ytdl.ToScreen(str(req.__dict__))
        res = self.cs_ytdl.PythonUrlOpen(req).GetAwaiter().GetResult()
        rx = FakeResponse(res, self.cs_ytdl)
        if not res.IsSuccessStatusCode:
            raise compat_urllib_error.HTTPError(rx.url, rx.status, rx.reason, rx.headers, rx)
        return rx

    def to_screen(self, message, skip_eol=False):
        self.cs_ytdl.ToScreen(message)
        return

    def report_warning(self, message):
        self.cs_ytdl.ReportWarning(message)
        return

    def report_error(self, message, tb=None):
        self.cs_ytdl.ReportError(message)
        return

    def _setup_opener(self):
        # temporary solution, going to be replaced by a c# version
        timeout_val = self.params.get('socket_timeout')
        self._socket_timeout = 600 if timeout_val is None else float(timeout_val)

        #opts_cookiefile = self.params.get('cookiefile')
        #opts_proxy = self.params.get('proxy')

        #if opts_cookiefile is None:
        #    self.cookiejar = compat_cookiejar.CookieJar()
        #else:
        #    opts_cookiefile = os.path.expandvars(compat_expanduser(opts_cookiefile))
        #    self.cookiejar = YoutubeDLCookieJar(opts_cookiefile)
        #    if os.access(opts_cookiefile, os.R_OK):
        #        self.cookiejar.load(ignore_discard=True, ignore_expires=True)

        #cookie_processor = YoutubeDLCookieProcessor(self.cookiejar)
        #if opts_proxy is not None:
        #    if opts_proxy == '':
        #        proxies = {}
        #    else:
        #        proxies = {'http': opts_proxy, 'https': opts_proxy}
        #else:
        #    proxies = compat_urllib_request.getproxies()
            # Set HTTPS proxy to HTTP one if given (https://github.com/ytdl-org/youtube-dl/issues/805)
        #    if 'http' in proxies and 'https' not in proxies:
        #        proxies['https'] = proxies['http']
        #proxy_handler = PerRequestProxyHandler(proxies)

        #debuglevel = 1 if self.params.get('debug_printtraffic') else 0
        #https_handler = make_HTTPS_handler(self.params, debuglevel=debuglevel)
        #ydlh = YoutubeDLHandler(self.params, debuglevel=debuglevel)
        #data_handler = compat_urllib_request_DataHandler()

        # When passing our own FileHandler instance, build_opener won't add the
        # default FileHandler and allows us to disable the file protocol, which
        # can be used for malicious purposes (see
        # https://github.com/ytdl-org/youtube-dl/issues/8227)
        #file_handler = compat_urllib_request.FileHandler()

        #def file_open(*args, **kwargs):
        #    raise compat_urllib_error.URLError('file:// scheme is explicitly disabled in youtube-dl for security reasons')
        #file_handler.file_open = file_open

        #opener = compat_urllib_request.build_opener(
        #    proxy_handler, https_handler, cookie_processor, ydlh, data_handler, file_handler)

        # Delete the default user-agent header, which would otherwise apply in
        # cases where our custom HTTP handler doesn't come into play
        # (See https://github.com/ytdl-org/youtube-dl/issues/1309 for details)
        #opener.addheaders = []
        #self._opener = opener


class FakeResponse():
    def __init__(self, httpResponse, ytdl):
        self.cs_res = httpResponse
        self.cs_ytdl = ytdl
        #self.cs_ytdl.ToScreen('__init__')
        self.status = int(self.cs_res.StatusCode)
        self.url = self.cs_res.RequestMessage.RequestUri.ToString()
        self.reason = self.cs_res.ReasonPhrase
        self.msg = self.reason

        self.headers = {}
        for kv in self.cs_res.Headers:
            if kv.Value.Length > 0:
                self.headers[kv.Key] = kv.Value[0]
            else:
                self.headers[kv.Key] = None
        return

    def geturl(self):
        #self.cs_ytdl.ToScreen('geturl')
        return self.url

    def info(self):
        #self.cs_ytdl.ToScreen('info')
        return None

    def getcode(self):
        #self.cs_ytdl.ToScreen('getcode')
        return self.status

    def getheader(self, name, default=None):
        #self.cs_ytdl.ToScreen('getheader')
        return self.headers.get(name, default)

    def getheaders(self):
        #self.cs_ytdl.ToScreen('getheaders')
        return self.headers

    def read(self, amt=None):
        #self.cs_ytdl.ToScreen('read')
        bytearr = self.cs_ytdl.PythonResponseToBytearray(self.cs_res).GetAwaiter().GetResult()
        #string = self.cs_res.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        #self.cs_ytdl.ToScreen('readtype: ' + bytearr.__class__.__name__)
        return bytearr


class FakeCookieJar():
    def __init__(self, ytdl):
        self.cs_ytdl = ytdl
        return

    def set_cookie(self, cookie):
        #self.cs_ytdl.ToScreen('set_cookie')
        self.cs_ytdl.SetCookie(cookie)
        return

    def add_cookie_header(self, req):
        #self.cs_ytdl.ToScreen('add_cookie_header')
        req.add_header('Cookie', self.cs_ytdl.GetCookie(req.get_full_url()))
        return

class FakeCache():
    def __init__(self, ytdl):
        self.cs_ytdl = ytdl
        return

    def load(self, section, key, dtype='json', default=None):
        return self.cs_ytdl.CacheLoad(section, key, default)
        return

    def store(self, section, key, data, dtype='json'):
        self.cs_ytdl.CacheStore(section, key, data)
        return