using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.WooCommerce.Helpers
{
    public static class MethodHelper
    {
        public static Predicate<Exception> TryAgainOnBadRequest = (ex) => ex.Message.Contains("BadRequest");

        public static Predicate<Exception> StopsOnTermExist = (ex) => !ex.Message.Contains("term_exists");

        public static async Task<Tuple<TResult, Exception>> ExecuteMethodAsync<TResult>(Func<Task<TResult>> func,
            int numberAttempts = 5, params Predicate<Exception>[] tryAgain)
        {
            var retry = true;
            var attempt = 0;
            TResult result = default;
            Exception exception = null;
            while (retry)
            {
                attempt += 1;
                exception = null;
                try
                {
                    result = await func();
                    retry = false;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    //retry = false;
                    retry = attempt < numberAttempts && tryAgain.Any(x => x(ex));
                    //tryAgain(ex) && attempt < numberAttempts;
                }

            }
            return Tuple.Create(result, exception);
        }

        public static Tuple<TResult, Exception> ExecuteMethod<TResult>(Func<TResult> func, Predicate<Exception> tryAgain, int numberAttempts = 5)
        {
            var retry = true;
            var attempt = 0;
            TResult result = default;
            Exception 
        exception = null;
            while (retry)
            {
                attempt += 1;
                exception = null;
                try
                {
                    result = func();
                    retry = false;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    retry = tryAgain(ex) && attempt < numberAttempts;
                }

            }
            return Tuple.Create(result, exception);
        }
    }
}
