using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PasswordManager.Infrastructure.Persistence
{
    class HintInterceptor : DbCommandInterceptor
    {
        private static readonly Regex _tableAliasRegex = new Regex(@"(?<tableAlias>FROM +(\[.*\]\.)?(\[.*\]) AS (\[.*\])(?! WITH \(*HINT*\)))", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [ThreadStatic]
        public static string HintValue;

        private static string Replace(string input)
        {
            if (!String.IsNullOrWhiteSpace(HintValue))
            {
                //if (!_tableAliasRegex.IsMatch(input))
                //{
                //    throw new InvalidProgramException("Unable to identify a table to be marked for update (forupdate)!", new Exception(input));
                //}
                //input = _tableAliasRegex.Replace(input, "${tableAlias} WITH (*HINT*)");
                //input = input.Replace("*HINT*", HintValue);
                if (HintValue == "FOR UPDATE")
                {
                    input = input + " for update ";
                }
            }
            HintValue = String.Empty;
            return input;
        }

        public override InterceptionResult<object> ScalarExecuting(DbCommand command,
        CommandEventData eventData, InterceptionResult<object> interceptionContext)
        {
            command.CommandText = Replace(command.CommandText);
            return interceptionContext;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData, InterceptionResult<DbDataReader> interceptionContext)
        {
            command.CommandText = Replace(command.CommandText);
            return interceptionContext;
        }
    }
}
