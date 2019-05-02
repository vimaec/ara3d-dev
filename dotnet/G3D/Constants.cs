namespace Ara3D
{
    /// <summary>
    /// The version identifier for this version of the G3D specification
    /// </summary>
    public class G3DVersion
    {
        public int Major = 9;
        public int Minor = 1;
        public int Revision = 0;
        public string Date = "2019-02-10";
    }

    /// <summary>
    /// The different types of data types that can be used as elements.
    /// </summary> 
    public enum DataType
    {
        dt_int8,
        dt_int16,
        dt_int32,
        dt_int64,
        dt_float32,
        dt_float64,
        dt_invalid,
    };

    // What element each attribute is associated with 
    public enum Association
    {
        assoc_vertex,
        assoc_face,
        assoc_corner, 
        assoc_edge,
        assoc_object,
        assoc_instance,
        assoc_group,
        assoc_none,
        assoc_invalid,
    };

    // The type of the attribute
    public enum AttributeType
    {
        attr_unknown,
        attr_vertex,
        attr_index,
        attr_faceindex,
        attr_facesize,
        attr_normal,
        attr_binormal,
        attr_tangent,
        attr_materialid,
        attr_polygroup,
        attr_uv,
        attr_color,
        attr_smoothing,
        //attr_crease,
        //attr_hole,
        attr_visibility,
        attr_selection,
        attr_pervertex,
        attr_mapchannel_data,
        attr_mapchannel_index,

        /// <summary>
        /// The instance transform, is a transform associated with each instance 
        /// </summary>
        attr_instance_transform,

        /// <summary>
        /// The instance group, is the index of the group associated with each instance
        /// </summary>
        attr_instance_group,

        /// <summary>
        /// This is the face index where each group starts. Groups can be nested.
        /// </summary>
        attr_group_index,

        /// <summary>
        /// This is the size of each group (number of faces)
        /// </summary>
        attr_group_size,

        /// <summary>
        /// Object ids 
        /// </summary>
        attr_object_id, 

        /// <summary>
        /// This is where each groups vertices start at.
        /// We assume no overlap, so the next group vertex offset delinerates the slice of vertices
        /// </summary>
        attr_group_vertex_offset,

        /// <summary>
        /// This is where each groups indices come from 
        /// We assume no overlap, so the next group index offset delineates the slice of indices
        /// </summary>
        attr_group_index_offset,

        attr_custom,
        attr_invalid,
    };
}