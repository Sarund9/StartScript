using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StartScript
{
    public class FunctionManager
    {




        public bool TryFunction(string fnname, out Closure closure)
        {
            closure = null;
            return false;
        }


    }


    public class EvaluationContext
    {
        Dictionary<string, object> vars
            = new Dictionary<string, object>();

        public object this[string key]
        {
            get => vars[key];
            set => vars[key] = value;
        }

        public bool EvaluateBody { get; set; }

    }

    public class EvaluationArgs
    {
        Arg[] args;
        int count;
        int namedIndex; // args[<-this] is named in call
        EvaluationContext ctx;

        public bool Positional<T>(out T value)
        {
            if (count >= args.Length)
            {
                value = default;
                return false;
            }
            var arg = args[count++];
            var result = arg.value.Evaluate(ctx);
            if (result is T t)
            {
                value = t;
                return true;
            }
            value = default;
            return false;
        }

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
            public IValueAST value;
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

    public struct EvaluationResult
    {
        public EvaluationResult(object value, bool append)
        {
            Value = value;
            Append = append;
        }
        public object Value { get; }
        public bool Append { get; }

        public static EvaluationResult Text(object value) =>
            new EvaluationResult(value, true);
        public static EvaluationResult Function(object value) =>
            new EvaluationResult(value, false);
        public static EvaluationResult Void { get; } =
            new EvaluationResult(null, false);
    }

    public interface IValueAST
    {
        EvaluationResult Evaluate(EvaluationContext ctx);
    }

    public class FunctionAST : IValueAST
    {
        List<IValueAST> implicitParams = new List<IValueAST>();
        List<(string, IValueAST)> explicitParams = new List<(string, IValueAST)>();

        public FunctionAST(string fnName)
        {
            FnName = fnName;
        }

        public IList<IValueAST> ImplicitParams => implicitParams;
        public IList<(string, IValueAST)> ExplicitParams => explicitParams;

        public string FnName { get; }

        public EvaluationResult Evaluate(EvaluationContext ctx)
        {
            // TODO: Call Functions
            throw new System.NotImplementedException();
        }
    }

    public class ValueAST : IValueAST
    {
        object value;
        public ValueAST(object value)
        {
            if (value == null)
                throw new System.ArgumentNullException(nameof(value));
            this.value = value ?? throw new System.ArgumentNullException(nameof(value));
        }
        public EvaluationResult Evaluate(EvaluationContext ctx) => EvaluationResult.Text(value);
    }

    /// <summary>
    /// Represents a Start Script Function
    /// </summary>
    public abstract class Closure
    {
        public Guid Id { get; } = Guid.NewGuid();

        public abstract EvaluationResult Evaluate(EvaluationContext ctx, EvaluationArgs args);

        public virtual void OnGUI(Rect area) { }
        public virtual float GetGUIHeight() => EditorGUIUtility.singleLineHeight;

        protected void ReEvaluate()
        {

        }

        protected void Error(string msg)
        {

        }
        protected void Log(string msg)
        {

        }
    }

    public class Message
    {

    }

    public class StartScriptClosureAttribute : Attribute
    {

    }

    [StartScriptClosure]
    public class If : Closure
    {
        // Insert this

        public override EvaluationResult Evaluate(EvaluationContext ctx, EvaluationArgs args)
        {
            if (!args.Positional<bool>(out var result))
            {
                Error("If must take a bool");
            }

            return EvaluationResult.Function(ctx.EvaluateBody = result);
        }
    }

    [StartScriptClosure]
    public class Toggle : Closure
    {
        // Insert this
        bool toggle;
        string toggleName;

        public override EvaluationResult Evaluate(EvaluationContext ctx, EvaluationArgs args)
        {
            if (!args.Positional<string>(out var name))
            {
                Error("Toggle requires an editor name");
            }
            toggleName = name;
            return EvaluationResult.Function(ctx.EvaluateBody = toggle);
        }

        // Override on GUI to get an Editor
        public override void OnGUI(Rect area)
        {
            EditorGUI.BeginChangeCheck();
            toggle = EditorGUI.Toggle(area, toggleName, toggle);
            if (EditorGUI.EndChangeCheck())
            {
                ReEvaluate();
            }
        }


    }

    [StartScriptClosure] // TODO: 
    public class Set : Closure
    {

        public override EvaluationResult Evaluate(EvaluationContext ctx, EvaluationArgs args)
        {
            var decl = args.GetNextNamed();
            if (decl == null)
            {
                Error("declaration has no");
            }
            // <var hello = 20 :>

            return EvaluationResult.Void;
        }

    }
}
