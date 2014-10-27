keyfob
======

Basic Authentication for Azure/IIS

Azure websites lack basic authentication. This is based on this code:

http://msdn.microsoft.com/en-us/library/vstudio/ms227673(v=vs.100).aspx

Unfortunately, if there's an error, the EndResponse won't properly read the cookie (or the cookie gets cleared). This provides a workaround for that scenario.
