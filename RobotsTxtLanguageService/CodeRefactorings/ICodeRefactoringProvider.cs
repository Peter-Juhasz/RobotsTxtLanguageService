using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace RobotsTxtLanguageService.CodeRefactorings
{
    public interface ICodeRefactoringProvider
    {
        IEnumerable<CodeAction> GetRefactorings(SnapshotSpan span);
    }
}
