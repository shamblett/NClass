using NClass.Core;

namespace NClass.Dart
{
    public interface IDartEventDeclaration : IEventDeclaration
    {
        bool IsExplicitImplementation { get; }
    }
}
