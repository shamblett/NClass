using System.Linq;
using System.Text.RegularExpressions;
using NClass.Core;
using NClass.Translations;

namespace NClass.Dart
{
    public class DartDestructorDeclaration : IDartDestructorDeclaration
    {
        // ~<name>()
        const string DestructorPattern =
            @"^\s*~(?<name>" + DartLanguage.NamePattern + ")" +
            @"\(\s*\)" + DartLanguage.DeclarationEnding;

        static readonly Regex destructorRegex =
            new Regex(DestructorPattern, RegexOptions.ExplicitCapture);

        private readonly Match match;

        public DartDestructorDeclaration(Match match)
        {
            if (!match.Success)
            {
                throw new BadSyntaxException(Strings.ErrorInvalidDeclaration);
            }

            this.match = match;
        }

        public static DartDestructorDeclaration Create(string declaration)
        {
            var match = destructorRegex.Match(declaration);
            return new DartDestructorDeclaration(match);
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
            get { return AccessModifier.Default; }
        }

        public IArgumentListDeclaration<IParameterDeclaration> ArgumentList
        {
            get { return new DartArgumentListDeclaration(Enumerable.Empty<DartParameterDeclaration>()); }
        }
    }
}
