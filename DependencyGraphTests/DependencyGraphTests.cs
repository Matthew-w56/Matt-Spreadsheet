
/// <summary>
/// This file contributes to the DevelopmentTests name-space with
/// a class that runs a series of tests of the DependencyGraph class.
/// These tests range from valid and invalid instantiations, to
/// ensuring correct dependency tracking.
/// 
/// Author: Matthew Williams
/// </summary>

using SpreadsheetUtilities;
using System.Diagnostics;

namespace DevelopmentTests {
    /// <summary>
    ///This is a test class for DependencyGraphTest and is intended
    ///to contain all DependencyGraphTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DependencyGraphTest {

        /// <summary>
        /// Tests the functionality of the Indexer method
        /// </summary>
        [TestMethod()]
        public void IndexerTest() {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("c", "d");
            //Should have a dependee
            Assert.AreEqual(1, graph["b"]);
            //Shouldn't have a dependee
            Assert.AreEqual(0, graph["a"]);
            //Isn't even in the graph
            Assert.AreEqual(0, graph["e"]);
        }

        /// <summary>
        /// Verifies the functionality of the HasDependents and HasDependees methods
        /// </summary>
        [TestMethod()]
        public void HasCheckerTests() {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("c", "d");
            graph.AddDependency("a", "d");

            //-----[ Test the DEPENDENTS methods ]-----
            //Should have a dependee
            Assert.IsTrue(graph.HasDependents("a"));
            //Should have a dependee
            Assert.IsTrue(graph.HasDependents("c"));
            //Should NOT have a dependee
            Assert.IsFalse(graph.HasDependents("b"));
            //Should NOT have a dependee
            Assert.IsFalse(graph.HasDependents("d"));
            //Doesn't even exist in graph
            Assert.IsFalse(graph.HasDependents("e"));

            //-----[ Test the DEPENDEES methods ]-----
            //Should have a dependee
            Assert.IsTrue(graph.HasDependees("d"));
            //Should have a dependee
            Assert.IsTrue(graph.HasDependees("b"));
            //Should NOT have a dependee
            Assert.IsFalse(graph.HasDependees("a"));
            //Should NOT have a dependee
            Assert.IsFalse(graph.HasDependees("c"));
            //Doesn't even exist in graph
            Assert.IsFalse(graph.HasDependees("e"));
        }

        /// <summary>
        /// Tests each possible situation with an Add
        /// </summary>
        [TestMethod()]
        public void ComprehensiveAddTest() {
            DependencyGraph graph = new DependencyGraph();
            //Both are new
            graph.AddDependency("a", "b");
            Assert.AreEqual(1, graph.Size);
            //Only dependent is new
            graph.AddDependency("a", "c");
            Assert.AreEqual(2, graph.Size);
            //Only dependee is new
            graph.AddDependency("d", "b");
            graph.AddDependency("e", "a");
            Assert.AreEqual(4, graph.Size);
            //Neither are new
            graph.AddDependency("a", "d");
            Assert.AreEqual(5, graph.Size);
        }

        /// <summary>
        /// Tests the Replace method when there are multiple dependencies in play.
        /// </summary>
        [TestMethod()]
        public void NextLevelReplaceTest() {
            //Create
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("c", "d");
            //Check size
            Assert.IsTrue(graph.Size == 2);
            //Add
            graph.AddDependency("a", "c");
            //Check size
            Assert.IsTrue(graph.Size == 3);
            //Replace c's dependent "d" with "a"
            graph.ReplaceDependents("c", new List<string> { "a" });
            Assert.IsTrue(graph.Size == 3);
            //Check to make sure "c"'s dependent is "a"
            IEnumerator<string> e1 = graph.GetDependents("c").GetEnumerator();
            Assert.IsTrue(e1.MoveNext());
            string val1 = e1.Current;
            Assert.AreEqual(val1, "a");
            //Replace back to D
            graph.ReplaceDependees("c", new List<string> { "d" });
            e1 = graph.GetDependents("c").GetEnumerator();
            Assert.IsTrue(e1.MoveNext());
            //Check again
            val1 = e1.Current;
            Assert.AreEqual(val1, "a");
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyTest() {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
        }
        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyRemoveTest() {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(1, t.Size);
            t.RemoveDependency("x", "y");
            Assert.AreEqual(0, t.Size);
        }
        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyEnumeratorTest() {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            IEnumerator<string> e1 = t.GetDependees("y").GetEnumerator();
            Assert.IsTrue(e1.MoveNext());
            Assert.AreEqual("x", e1.Current);
            IEnumerator<string> e2 = t.GetDependents("x").GetEnumerator();
            Assert.IsTrue(e2.MoveNext());
            Assert.AreEqual("y", e2.Current);
            t.RemoveDependency("x", "y");
            Assert.IsFalse(t.GetDependees("y").GetEnumerator().MoveNext());
            Assert.IsFalse(t.GetDependents("x").GetEnumerator().MoveNext());
        }
        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void SimpleReplaceTest() {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(1, t.Size);
            t.RemoveDependency("x", "y");
            t.ReplaceDependents("x", new HashSet<string>());
            t.ReplaceDependees("y", new HashSet<string>());
        }
        ///<summary>
        ///It should be possible to have more than one DG at a time.
        ///</summary>
        [TestMethod()]
        public void StaticTest() {
            DependencyGraph t1 = new DependencyGraph();
            DependencyGraph t2 = new DependencyGraph();
            t1.AddDependency("x", "y");
            Assert.AreEqual(1, t1.Size);
            Assert.AreEqual(0, t2.Size);
        }
        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void SizeTest() {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");
            Assert.AreEqual(4, t.Size);
        }
        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void EnumeratorTest() {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");
            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());
            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));
            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());
            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }
        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void ReplaceThenEnumerate() {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "b");
            t.AddDependency("a", "z");
            t.ReplaceDependents("b", new HashSet<string>());
            t.AddDependency("y", "b");
            t.ReplaceDependents("a", new HashSet<string>() { "c" });
            t.AddDependency("w", "d");
            t.ReplaceDependees("b", new HashSet<string>() { "a", "c" });
            t.ReplaceDependees("d", new HashSet<string>() { "b" });
            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());
            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));
            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());
            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }
        /// <summary>
        ///Using lots of data
        ///</summary>
        [TestMethod()]
        public void StressTest() {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();
            // A bunch of strings to use
            const int SIZE = 26;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++) {
                letters[i] = "" + (char)('a' + i);
            }
            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++) {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }
            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++) {
                for (int j = i + 1; j < SIZE; j++) {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }
            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++) {
                for (int j = i + 4; j < SIZE; j += 4) {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }
            // Add some back
            for (int i = 0; i < SIZE; i++) {
                for (int j = i + 1; j < SIZE; j += 2) {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }
            // Remove some more
            for (int i = 0; i < SIZE; i += 2) {
                for (int j = i + 3; j < SIZE; j += 3) {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++) {
                Assert.IsTrue(dents[i].SetEquals(new
        HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new
        HashSet<string>(t.GetDependees(letters[i]))));
            }
        }
    }
}