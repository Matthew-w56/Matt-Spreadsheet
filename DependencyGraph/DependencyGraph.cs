using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SpreadsheetUtilities {
    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
  public class DependencyGraph {

        private Dictionary<string, HashSet<string>> dependents;
        private Dictionary<string, HashSet<string>> dependees;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph() {
            dependents = new();
            dependees = new();
        }

        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size {
            get {
                //Add up the count of each pair in one of the sets (dependents used here arbitrarily)
                int totalSize = 0;
                foreach (HashSet<string> val in dependents.Values) {
                    totalSize += val.Count;
                }
                return totalSize;
            }
        }

        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s] {
            get { return (dependees.ContainsKey(s) ? dependees[s].Count : 0); }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s) {
            return dependents.ContainsKey(s);
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s) {
            return dependees.ContainsKey(s);
        }

        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s) {
            if (dependents.ContainsKey(s) && dependents[s] != null) return dependents[s];
            return new HashSet<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s) {
            if (dependees.ContainsKey(s) && dependees[s] != null) return dependees[s];
            return new HashSet<string>();
        }

        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>
        public void AddDependency(string s, string t) {
            //If s already has dependents, add t to the list.  Else, create new entry
            if (dependents.ContainsKey(s)) dependents[s].Add(t);
            else dependents.Add(s, new HashSet<string> { t });

            //If t already has dependees, add s to the list.  Else, create new entry.
            if (dependees.ContainsKey(t)) dependees[t].Add(s);
            else dependees.Add(t, new HashSet<string> { s });
        }

        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t) {
            //If the link exists, remove it.
            if (dependents.ContainsKey(s)) {
                if (dependents[s].Count < 2) dependents.Remove(s);
                else dependents[s].Remove(t);
            }
            if (dependees.ContainsKey(t)) {
                if (dependees[t].Count < 2) dependees.Remove(t);
                else dependees[t].Remove(s);
            }
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents) {
            //Remove any existing dependents of s
            if (dependents.ContainsKey(s)) {
                List<string> items = dependents[s].ToList();
                foreach (string item in items) {
                    RemoveDependency(s, item);
                }
            }

            //Add all desired dependents to s
            foreach (string t in newDependents) {
                AddDependency(s, t);
            }
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees) {
            //Remove any existing dependents of s
            if (dependees.ContainsKey(s)) {
                List<string> items = dependees[s].ToList();
                foreach (string item in items) {
                    RemoveDependency(item, s);
                }
            }

            //Add all desired dependents to s
            foreach (string t in newDependees) {
                AddDependency(t, s);
            }
        }
    }
}