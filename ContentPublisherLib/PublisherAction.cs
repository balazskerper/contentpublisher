using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPublisherLib
{
    public class PublisherAction : IEquatable<PublisherAction>
    {
        public PublisherAction()
        {

        }

        public PublisherAction(PublisherTypes.ActionType type, PublisherActionResult result, string source, string target)
        {
            Type = type;
            Result = result;
            SourcePath = source;
            TargetPath = target;
        }

        public PublisherTypes.ActionType Type { get; set; }
        public PublisherActionResult Result { get; set; }

        public string SourcePath { get; set; }
        public string TargetPath { get; set; }

        public override string ToString()
        {
            StringBuilder actionstring = new StringBuilder();
            if (Type.GetDescription() != null)
            {
                actionstring.Append(Type.GetDescription());
            }
            else
            {
                actionstring.Append("UnknownType");
            }
            actionstring.Append($";{SourcePath}");
            actionstring.Append($";{TargetPath}");
            actionstring.Append($";{Result.ResultType}");
            if (Result.ResultMessage != null)
            {
                actionstring.Append($";{Result.ResultMessage}");
            }
            return actionstring.ToString();
        }

        public bool Equals(PublisherAction other)
        {
            return other != null &&
                    Type == other.Type &&
                    EqualityComparer<PublisherActionResult>.Default.Equals(Result, other.Result) &&
                    SourcePath == other.SourcePath &&
                    TargetPath == other.TargetPath;
        }

        public override int GetHashCode()
        {
            int hashType = Type.GetHashCode();
            int hashSourcePath = SourcePath == null ? 0 : SourcePath.GetHashCode();
            int hashTargetPath = TargetPath == null ? 0 : TargetPath.GetHashCode();

            return hashSourcePath ^ hashTargetPath ^ hashType;
        }
    }

    public struct PublisherActionResult
    {
        public PublisherTypes.ActionResultType ResultType { get; private set; }
        public string ResultMessage { get; private set; }

        public PublisherActionResult(PublisherTypes.ActionResultType _type)
        {
            ResultType = _type;
            ResultMessage = null;
        }

        public PublisherActionResult(PublisherTypes.ActionResultType type, string message)
        {
            ResultType = type;
            ResultMessage = message;
        }
    }
}
