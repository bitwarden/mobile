﻿using System;
using System.Collections.Generic;
using Bit.App.Resources;
using Bit.App.Utilities;
using System.Linq;

namespace Bit.App.Models.Page
{
    public class VaultListPageModel
    {
        public class Site
        {
            public Site(Models.Site site, string folderId)
            {
                Id = site.Id;
                FolderId = folderId;
                Name = site.Name?.Decrypt();
                Username = site.Username?.Decrypt() ?? " ";
                Password = new Lazy<string>(() => site.Password?.Decrypt());
                Uri = new Lazy<string>(() => site.Uri?.Decrypt());
            }

            public string Id { get; set; }
            public string FolderId { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }
            public Lazy<string> Password { get; set; }
            public Lazy<string> Uri { get; set; }
        }

        public class Folder : List<Site>
        {
            public Folder(IEnumerable<Models.Site> sites, string folderId = null)
            {
                var pageSites = sites.Select(s => new Site(s, folderId)).OrderBy(s => s.Name);
                AddRange(pageSites);
            }

            public Folder(Models.Folder folder, IEnumerable<Models.Site> sites)
                : this(sites, folder.Id)
            {
                Id = folder.Id;
                Name = folder.Name?.Decrypt();
            }

            public string Id { get; set; }
            public string Name { get; set; } = AppResources.FolderNone;
        }
    }
}
