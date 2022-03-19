using System.Text.RegularExpressions;
using NClass.Core;
using NClass.Translations;

namespace NClass.Dart
{
    public class DartConstructorDeclaration : IDartConstructorDeclaration
    {
        // [<access>] [static] <name>([<args>])
        const string ConstructorPattern =
            @"^\s*" + DartLanguage.AccessPattern + @"(?<type>static|factory\s+)?" +
            @"(?<name>" + DartLanguage.NamePattern + ")" +
            @"\((?(static)|(?<args>.*))\)" + DartLanguage.DeclarationEnding;

        static readonly Regex constructorRegex =
            new Regex(ConstructorPattern, RegexOptions.ExplicitCapture);

        private readonly Match match;

        public DartConstructorDeclaration(Match match)
        {
            if (!match.Success)
            {
                throw new BadSyntaxException(Strings.ErrorInvalidDeclaration);
            }

            this.match = match;
        }

        public static DartConstructorDeclaration Create(string declaration)
        {
            var match = constructorRegex.Match(declaration);
            return new DartConstructorDeclaration(match);
        }

        public string Name
        {
            get { return match.Groups["name"].Value; }
        }

        public string Type
        {
            get { return string.Empty; }
        }

        public AccessModifier AccessModifier
        {
            get { return DartLanguage.Instance.TryParseAccessModifier(match.Groups["access"].Value); }
        }

        public IArgumentListDeclaration<IParameterDeclaration> ArgumentList
        {
            get { return DartArgumentListDeclaration.Create(match.Groups["args"].Value); }
        }

        public bool IsStatic
        {
            get { return match.Groups["static"].Success; }
        }

        public bool IsFactory
        {
            get { return match.Groups["factory"].Success; }
        }
    }
}
