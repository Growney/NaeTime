using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace EventSourcingCore.Readiness.Abstractions
{
    public struct ReadinessResult
    {
        public ReadinessResult(string header, ReadinessResultBody body)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Body = Single(body);
        }
        public ReadinessResult(string header, IEnumerable<ReadinessResultBody> body)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        private static IEnumerable<ReadinessResultBody> Single(ReadinessResultBody body)
        {
            yield return body;
        }

        public string Header { get; }
        public IEnumerable<ReadinessResultBody> Body { get; }

        public bool IsReady()
        {
            foreach (var body in Body)
            {
                if (!IsReady(body))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsReady(ReadinessResultBody body)
        {
            if (body.Success)
            {
                if (body.Body != null)
                {
                    foreach (var b in body.Body)
                    {
                        if (!IsReady(b))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return true;
            }
            return false;
        }

    }
}
