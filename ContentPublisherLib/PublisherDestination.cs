using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ContentPublisherLib
{
    public class PublisherDestination
    {
        public string TargetPath { get; set; }
        public PublisherTypes.PublisherTargetType TargetType { get; set; }
        public List<PublisherTypes.PublisherMode> Modes { get; set; }


        public PublisherDestination()
        {
            Modes = new List<PublisherTypes.PublisherMode>();
        }

        public static PublisherDestination Create(XElement destelement)
        {
            if (destelement.Attribute("path") != null)
            {
                PublisherDestination destination = new PublisherDestination
                {
                    TargetPath = destelement.Attribute("path").Value
                };
                if (destelement.Attribute("mode") != null)
                {
                    foreach (string mode in destelement.Attribute("mode").Value.Split(' '))
                    {
                        switch (mode)
                        {
                            case "overwrite":
                                destination.Modes.Add(PublisherTypes.PublisherMode.Overwrite);
                                break;
                            case "archive":
                                destination.Modes.Add(PublisherTypes.PublisherMode.Archive);
                                break;
                            case "subonly":
                                destination.Modes.Add(PublisherTypes.PublisherMode.SubOnly);
                                break;
                        }

                    }
                }
                if (destelement.Attribute("type") != null)
                {
                    switch (destelement.Attribute("type").Value)
                    {
                        case "file":
                            destination.TargetType = PublisherTypes.PublisherTargetType.File;
                            break;
                        case "folder":
                            destination.TargetType = PublisherTypes.PublisherTargetType.Directory;
                            break;
                        case "archive":
                            destination.TargetType = PublisherTypes.PublisherTargetType.Archive;
                            break;
                    }
                }
                return destination;
            } else {
                return null;
            }

        }

        public override string ToString()
        {
            StringBuilder deststring = new StringBuilder();
            deststring.AppendLine($"Target: {this.TargetPath}");
            switch (TargetType)
            {
                case PublisherTypes.PublisherTargetType.File:
                    deststring.AppendLine($"TType : file");
                    break;
                case PublisherTypes.PublisherTargetType.Directory:
                    deststring.AppendLine($"TType : folder");
                    break;
                case PublisherTypes.PublisherTargetType.Archive:
                    deststring.AppendLine($"TType : archive");
                    break;
            }
            if (Modes != null)
            {
                deststring.AppendLine($"TModes:");
                foreach (PublisherTypes.PublisherMode mode in Modes)
                {
                    switch (mode)
                    {
                        case PublisherTypes.PublisherMode.Archive:
                            deststring.AppendLine("*archive");
                            break;
                        case PublisherTypes.PublisherMode.Overwrite:
                            deststring.AppendLine("*overwrite");
                            break;
                        case PublisherTypes.PublisherMode.SubOnly:
                            deststring.AppendLine("*subonly");
                            break;
                    }
                }
            }
            return deststring.ToString();
        }
    }
}
