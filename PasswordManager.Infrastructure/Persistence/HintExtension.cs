using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Infrastructure.Persistence
{
    public static class HintExtension
    {
        public static IQueryable<T> WithHint<T>(this IQueryable<T> set, string hint) where T : class
        {
            HintInterceptor.HintValue = hint;
            return set;
        }
        public static IQueryable<T> ForUpdate<T>(this IQueryable<T> set) where T : class
        {
            return set.WithHint("FOR UPDATE");
        }
    }
}
