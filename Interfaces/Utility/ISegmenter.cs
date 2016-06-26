using System.Collections.Generic;

namespace OwinFramework.Interfaces.Utility
{
    /// <summary>
    /// Implements a segmentation algorithm. Given a tree of route segments and a 
    /// list of nodes calculates which nodes should be on which segments of the 
    /// route such that any path through the route tree will satisfy the dependencies
    /// of all nodes.
    /// </summary>
    public interface ISegmenter
    {
        /// <summary>
        /// Removes all the nodes and segments from the segmenter
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds a node to be segmented
        /// </summary>
        /// <param name="key">A unique identifier for this node</param>
        /// <param name="dependencies">A list of dependencies, each dependency comprises a
        /// list of keys that can be used to satisfy the dependency. All dependencies have 
        /// to be met.</param>
        /// <param name="segments">A list of the names of the route segments that must 
        /// include this node when traversed</param>
        void AddNode(string key, IEnumerable<IList<string>> dependencies = null, IEnumerable<string> segments = null);

        /// <summary>
        /// Adds a route segment
        /// </summary>
        /// <param name="name">The unique name of this segment</param>
        /// <param name="childSegments">The names of the segments that are
        /// under this one in the routing tree</param>
        void AddSegment(string name, IEnumerable<string> childSegments = null);

        /// <summary>
        /// Figures out shich nodes should be in a given segment
        /// </summary>
        /// <param name="segmentName">The name of the segment</param>
        /// <returns>A list of the node key values for this segment</returns>
        IList<string> GetSegmentNodes(string segmentName);

        /// <summary>
        /// Figures out which segments a node should be in
        /// </summary>
        /// <param name="nodeKey">The unique key identifying a node</param>
        /// <returns>A list of the names of the segments that this node is in</returns>
        IList<string> GetNodeSegments(string nodeKey);
    }

}

