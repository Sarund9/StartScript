using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StartScript
{
    public class ClosureManager
    {




        public bool TryFunction(string fnname, out Closure closure)
        {
            closure = null;
            return false;
        }


    }

    public class EvaluationArgs
    {
        Arg[] args;
        int count;
        int namedIndex; // args[<-this] is named in call
        //EvaluationContext ctx;

        public bool Positional<T>(out T value)
        {
            if (count >= args.Length)
            {
                value = default;
                return false;
            }
            var arg = args[count++];
            var result = arg.value.Evaluate();
            if (result is T t)
            {
                value = t;
                return true;
            }
            value = default;
            return false;
        }

        // TODO: 
        public (string name, object value)? GetNextNamed()
        {
            if (namedIndex >= args.Length)
            {
                return null;
            }


            return null;
        }

        struct Arg
        {
            public IExpr value;
            public string name;
        }

        static string NameOfCallingClass()
        {
            string fullName;
            Type declaringType;
            int skipFrames = 2;
            do
            {
                MethodBase method = new StackFrame(skipFrames, false).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    return method.Name;
                }
                skipFrames++;
                fullName = declaringType.FullName;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return fullName;
        }
    }

    public struct Evaluation
    {
        public Evaluation(object value, bool append)
        {
            Value = value;
            Append = append;
        }
        public object Value { get; }
        public bool Append { get; }

        public static Evaluation Text(object value) =>
            new Evaluation(value, true);
        public static Evaluation Function(object value) =>
            new Evaluation(value, false);
        public static Evaluation Void { get; } =
            new Evaluation(null, false);
    }

    public interface IExpr
    {
        Evaluation Evaluate();
    }

    public class ValueExpr : IExpr
    {
        object value;
        public ValueExpr(object value)
        {
            if (value == null)
                throw new System.ArgumentNullException(nameof(value));
            this.value = value ?? throw new System.ArgumentNullException(nameof(value));
        }
        public Evaluation Evaluate() => Evaluation.Text(value);
    }

}
