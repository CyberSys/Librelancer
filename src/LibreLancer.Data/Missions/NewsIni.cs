// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Collections.Generic;
using LibreLancer.Ini;

namespace LibreLancer.Data.Missions
{
    public class NewsIni : IniFile
    {
        [Section("NewsItem")] public List<NewsItem> NewsItems = new List<NewsItem>();
        public void AddNewsIni(string path, FileSystem vfs)
        {
            ParseAndFill(path, vfs);
        }
    }

    public class NewsItem
    {
        [Entry("rank")] public string[] Rank;
        [Entry("icon")] public string Icon;
        [Entry("logo")] public string Logo;
        [Entry("category")] public int Category;
        [Entry("headline")] public int Headline;
        [Entry("text")] public int Text;
        [Entry("base", Multiline = true)] public List<string> Base = new List<string>();
        [Entry("autoselect", Presence = true)] public bool Autoselect;
    }
}