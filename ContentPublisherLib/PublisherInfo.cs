using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ContentPublisherLib
{
    public class PublisherInfo
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public PublisherTypes.PublisherSourceType Type { get; set; }
        public List<PublisherTypes.PublisherMode> Modes { get; set; }

        public List<PublisherDestination> Destinations { get; set; }

        public PublisherInfo()
        {
            Modes = new List<PublisherTypes.PublisherMode>();
            Destinations = new List<PublisherDestination>();
        }

        public PublisherInfo(XElement element) : this()
        {
            if (element != null && element.Name.ToString().Equals("source"))
            {
                if (element.Attribute("name") != null)
                {
                    Name = element.Attribute("name").Value;
                }
                if (element.Attribute("path") != null)
                {
                    SourcePath = element.Attribute("path").Value;
                }
                if (element.Attribute("type") != null)
                {
                    switch (element.Attribute("type").Value)
                    {
                        case "file":
                            Type = PublisherTypes.PublisherSourceType.File;
                            break;
                        case "folder":
                            Type = PublisherTypes.PublisherSourceType.Directory;
                            break;
                    }
                }
                if (element.Attribute("mode") != null)
                {
                    foreach (string mode in element.Attribute("mode").Value.Split(' '))
                    {
                        switch (mode)
                        {
                            case "overwrite":
                                Modes.Add(PublisherTypes.PublisherMode.Overwrite);
                                break;
                            case "archive":
                                Modes.Add(PublisherTypes.PublisherMode.Archive);
                                break;
                            case "subonly":
                                Modes.Add(PublisherTypes.PublisherMode.SubOnly);
                                break;
                        }

                    }
                }
                if (element.Elements("destination") != null)
                {
                    foreach (XElement destelement in element.Elements("destination"))
                    {
                        PublisherDestination destination = PublisherDestination.Create(destelement);

                        if (destination != null)
                        {
                            Destinations.Add(destination);
                        }
                    }
                }
            }
        }
        public override string ToString()
        {
            StringBuilder pubinfo = new StringBuilder();
            pubinfo.AppendLine($"Name  : {Name}");
            pubinfo.AppendLine($"Source: {SourcePath}");
            pubinfo.AppendLine($"Type  : {Type}");
            if (Modes != null)
            {
                pubinfo.AppendLine($"Modes :");
                foreach (PublisherTypes.PublisherMode mode in Modes)
                {
                    switch (mode)
                    {
                        case PublisherTypes.PublisherMode.Archive:
                            pubinfo.AppendLine("*archive");
                            break;
                        case PublisherTypes.PublisherMode.Overwrite:
                            pubinfo.AppendLine("*overwrite");
                            break;
                        case PublisherTypes.PublisherMode.SubOnly:
                            pubinfo.AppendLine("*subonly");
                            break;
                    }
                }
            }
            if (Destinations != null)
            {
                foreach (PublisherDestination dest in Destinations)
                {
                    pubinfo.Append(dest.ToString());
                }
            }
            return pubinfo.ToString();
        }
    }
}
