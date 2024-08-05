using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StartScript
{
    public class GroupedStack<T> : IEnumerable<(T value, int group)>
    {
        Stack<T> stack = new Stack<T>();
        Stack<int> groups = new Stack<int>();

        public void Add(T item)
        {
            stack.Push(item);
        }

        public void EnterScope()
        {
            groups.Push(stack.Count);
        }

        /*
        Actions:
        - add
        - add
        - open scope
        - add
        Stack:
        - item
        - item
        - 
        Groups:
        - 2

        */

        public void ExitScope(Action<T> onremoved)
        {
            if (groups.Count == 0)
            {
                stack.ForEach(onremoved);
                stack.Clear();
                return;
            }

            int groupCount = groups.Pop();
            for (int i = stack.Count - 1;
                i >= groupCount; i--)
            {
                onremoved(stack.Pop());
            }
        }

        public IEnumerator<(T, int)> GetEnumerator()
        {
            var gArr = groups.ToArray();
            int gi = 0;
            int i = 0;
            foreach (var item in stack.Reverse())
            {
                yield return (item, gArr.Length - gi);
                if (gi < gArr.Length && i == gArr[gi] - 1)
                {
                    gi++;
                }
                i++;
            }
            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Match<T>
    {
        class Node
        {
            public Predicate<T> cond;
            public Func<T, T> selector;
            public Action action;
            public Stack<Node> SubStack { get; } = new Stack<Node>();
            public Node parent;
        }

        Node root = new Node
        {
            cond = x => true,
            selector = null,
        };
        Node current;

        public void Clear()
        {
            root.SubStack.Clear();
            current = null;
        }

        public Match<T> Case(Predicate<T> cond, Func<T, T> selector = null)
        {
            var node = new Node
            {
                cond = cond,
                selector = selector,
            };
            if (current is null)
                current = root;
            
            current.SubStack.Push(node);
            node.parent = current;
            current = node;

            return this;
        }

        public Match<T> Do(Action action)
        {
            current.action += action;
            return this;
        }

        public Match<T> End()
        {
            current = current.parent;
            return this;
        }

        public void Execute(T value)
        {
            bool valid = false;
            ExecuteCore(root, ref valid, value);
        }

        private void ExecuteCore(Node node, ref bool valid, T value)
        {
            valid = node.cond(value);
            if (!node.cond(value))
                return;

            node.action?.Invoke();

            foreach (var item in node.SubStack)
            {
                ExecuteCore(item, ref valid,
                    node.selector is null ?
                        value :
                        node.selector(value));
            }
        }

        static void Test()
        {
            var match = new Match<int>()
                .Case(x => x == 0)
                   .Do(() => { })
                .End()
                .Case(x => x > 5)
                    .Case(x => x > 5)
                        .Do(() => { })
                    .End()
                .End();
        }
    }

    #region OLD
    /*
        Stack<T> stack = new Stack<T>();
        bool valid = true;

        public T Current
        {
            get => stack.Peek();
            private set => stack.Push(value);
        }

        public static Match<T> From(T value)
        {
            return new Match<T> { Current = value };
        }

        public Match<T> Reset(T value)
        {
            stack.Clear();
            Current = value;
            valid = true;
            return this;
        }

        public Match<T> Case(Predicate<T> cond, Func<T, T> selector = null)
        {
            valid &= cond(Current);
            if (selector != null)
                stack.Push(selector(Current));
            return this;
        }

        public Match<T> Do(Action action)
        {
            if (valid)
                action();
            return this;
        }

        public Match<T> End()
        {
            stack.Pop();
            return this;
        }

        static void Test()
        {
            var test = new Match<int>();

            test.Reset(0)
                .Case(x => x == 0)
                    .Do(() => { })
                .End();
        }
    */
    #endregion

    //public class 
}
