using System.Collections.Generic;
using DocSaw.Confluence;

namespace DocSaw
{
    public class ErrorReporter
    {
        private readonly List<Error> _errors;

        public int ErrorsCount => _errors.Count;

        public ErrorReporter()
        {
            _errors = new List<Error>();
        }

        public void Report(Page page, string message)
        {
            _errors.Add(new Error
            {
                Page = page,
                Message = message
            });
        }

        public void SendErrorsTo(IErrorSender target)
        {
            target.Send(_errors);
        }

        public class Error
        {
            public Page Page { get; set; }
            public string Message { get; set; }
        }
    }

    public interface IErrorSender
    {
        void Send(IEnumerable<ErrorReporter.Error> errors);
    }
}