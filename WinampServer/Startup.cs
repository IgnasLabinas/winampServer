﻿using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

[assembly: Microsoft.Owin.OwinStartup(typeof(Startup))]
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        var physicalFileSystem = new PhysicalFileSystem("./Client");
        var options = new FileServerOptions
        {
            EnableDefaultFiles = true,
            FileSystem = physicalFileSystem
        };
        options.StaticFileOptions.FileSystem = physicalFileSystem;
        options.StaticFileOptions.ServeUnknownFileTypes = true;
        options.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" };
        app.UseFileServer(options);

        app.UseCors(CorsOptions.AllowAll);
        app.MapSignalR();
    }
}