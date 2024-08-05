using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StartScript
{
    /// <summary>
    /// Represents a Start Script Function
    /// </summary>
    public abstract class Closure : IExpr
    {
        
        public abstract ClArg[] Sign { get; }

        public abstract ScopeMode Scoping { get; }

        public VariableCollection Variables
        {
            get; internal set;
        }

        public Evaluation Evaluate()
        {
            var args = new EvaluationArgs();

            return EvaluateClosure(args);
        }

        protected abstract Evaluation EvaluateClosure(EvaluationArgs args);

        public virtual void OnEditorGUI(Rect area) { }
        public virtual float GetEditorHeight() => EditorGUIUtility.singleLineHeight;

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

    public struct ClArg
    {
        public Type Type { get; private set; }
        public bool IsNullable { get; private set; }
        public bool HasDefault { get; private set; }
        public object DefaultValue { get; private set; }
        public string ArgKeyword { get; private set; }

        public static ClArg[] None { get; } = new ClArg[0];

        public static ClArg[] Arr(params ClArg[] arr) => arr;

        public static ClArg Positional<T>(string kw = null) => new ClArg
        {
            Type = typeof(T),
        };

        public static ClArg Keyword<T>(string kw) => new ClArg
        {
            Type = typeof(T),
            ArgKeyword = kw,
        };

        public ClArg Default(object value)
        {
            var self = this;
            self.DefaultValue = value;
            return self;
        }

        public ClArg Nullable
        {
            get
            {
                var self = this;
                self.IsNullable = true;
                return self;
            }
        }
    }

    public enum ScopeMode
    {
        Unscoped,
        Scoped,
        Optional,
    }

    public class VariableCollection : IEnumerable<KeyValuePair<string, object>>
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        GroupedStack<string> scopes = new GroupedStack<string>();

        public object this[string key]
        {
            get => vars.ContainsKey(key) ? vars[key] : null;
            set {
                scopes.Add(key);
                vars[key] = value;
            }
        }

        public int Count => vars.Count;

        public bool Exists(string var) => vars.ContainsKey(var);

        public void BeginScope()
        {
            scopes.EnterScope();
        }
        public void EndScope()
        {
            //Debug.Log("END SCOPE");
            scopes.ExitScope(x => vars.Remove(x));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            ((IEnumerable<KeyValuePair<string, object>>)vars).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)vars).GetEnumerator();
    }

    // TODO: ConstVariableCollection

    public class StartScriptClosureAttribute : Attribute
    {


        public bool HasEditor { get; }
    }


    class Set : Closure
    {
        public override ClArg[] Sign { get; } = ClArg.Arr(
            ClArg.Positional<Name>(),
            ClArg.Positional<object>()
        );

        // Set var1 "MyVar"

        public override ScopeMode Scoping => ScopeMode.Unscoped;

        protected override Evaluation EvaluateClosure(EvaluationArgs args)
        {
            args.Positional<Name>(out var name);
            args.Positional<object>(out var value);

            Variables[name.Value] = value;

            return Evaluation.Void;
        }
    }

    public struct Name
    {
        public Name(string value)
        {
            Value = value;
        }
        public string Value { get; }
    }
}

#region OLD CODE
/*


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


public class FlowBehaviour
{
    HashSet<Type> closing = new HashSet<Type>();
        
    public bool AllowFullScope { get; private set; }
    public bool AllowInlinedScope { get; private set; }
    public bool AllowScopeless { get; private set; }

    public bool IsClosedBy(Type t) => closing.Contains(t);
        
    public static FlowBehaviour Allow => new FlowBehaviour();
        
    #region Flag Builder

    public FlowBehaviour FullScope()
    {
        AllowFullScope = true;
        return this;
    }
    public FlowBehaviour InlinedScope()
    {
        AllowInlinedScope = true;
        return this;
    }
    public FlowBehaviour Scopeless()
    {
        AllowScopeless = true;
        return this;
    }
    public FlowBehaviour All()
    {
        AllowFullScope = true;
        AllowInlinedScope = true;
        AllowScopeless = true;
        return this;
    }

    #endregion

    #region Close Builder

    public FlowBehaviour ClosedBy<T>()
    {
        closing.Add(typeof(T));
        return this;
    }

    #endregion
}


*/
#endregion
