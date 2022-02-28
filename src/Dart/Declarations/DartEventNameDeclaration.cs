using NClass.Core;
using NClass.Translations;
using System.Text.RegularExpressions;

namespace NClass.Dart
{
    public class DartEventNameDeclaration
    {
        const string EventNamePattern =
            @"^\s*(?<name>" + DartLanguage.GenericOperationNamePattern + @")\s*$";

        static readonly Regex nameRegex = new Regex(EventNamePattern, RegexOptions.ExplicitCapture);

        readonly Match match;

        public DartEventNameDeclaration(Match match)
        {
            if (!match.Success)
            {
                throw new BadSyntaxException(Strings.ErrorInvalidDeclaration);
            }

            this.match = match;
        }

        public static DartEventNameDeclaration Create(string declaration)
        {
            var match = nameRegex.Match(declaration);
            return new DartEventNameDeclaration(match);
        }

        public string Name
        {
            get { return match.Groups["name"].Value; }
        }

        public bool IsExplicitImplementation
        {
            get { return match.Groups["namedot"].Success; }
        }
    }
}
