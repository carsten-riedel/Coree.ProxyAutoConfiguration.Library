using Jint.Native;
using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coree.ProxyAutoConfiguration.Library
{
    public class JintScriptRunner
    {
        public enum ExecutionState
        {
            Success,
            FunctionNonInvokable,
            ScriptMissing,
            FunctionNotFound,
            ScriptExecutionError
        }

        public class Result
        {
            public string? ReturnValue { get; set; }
            public ExecutionState State { get; set; }
            public Exception? Exception { get; set; }
        }

        public static Result Execute(string? script, string function, params object[] jsValues)
        {
            var result = new Result();

            if (string.IsNullOrEmpty(script))
            {
                result.State = ExecutionState.ScriptMissing;
                return result;
            }

            try
            {
                Engine engine = new Engine();
                engine.Execute(script);

                var func = engine.GetValue(function);

                if (func.IsUndefined())
                {
                    result.State = ExecutionState.FunctionNotFound;
                    return result;
                }

                if (!func.IsObject())
                {
                    result.State = ExecutionState.FunctionNonInvokable;
                    return result;
                }

                JsValue[] jsValuesConverted = jsValues.Select(v => JsValue.FromObject(engine, v)).ToArray();
                JsValue jsResult = engine.Invoke(func, jsValuesConverted);

                result.ReturnValue = jsResult.ToString();
                result.State = ExecutionState.Success;
                return result;
            }
            catch (Exception ex)
            {
                result.State = ExecutionState.ScriptExecutionError;
                result.Exception = ex;
                return result;
            }
        }
    }
}
