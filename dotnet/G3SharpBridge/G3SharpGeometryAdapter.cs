using g3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D
{
    public class G3SharpGeometryAdapter
    {
        public IGeometry Source { get; private set; }

        public IGeometry Result { get; private set; }

        // TODO: if the vertices and indices are the same, then this could be accelerated. 

        private readonly MemoizedFunction<IGeometry, DMesh3> GeometryToDMesh 
            = new MemoizedFunction<IGeometry, DMesh3>((g) => g.ToG3Sharp());

        private readonly MemoizedFunction<DMesh3, Reducer> DMeshToReducer
            = new MemoizedFunction<DMesh3, Reducer>((m) => new Reducer(new DMesh3(m)));

        private readonly MemoizedFunction<DMesh3, DMeshAABBTree3> DMeshToAABBTree
            = new MemoizedFunction<DMesh3, DMeshAABBTree3>((m) => m.AABBTree());

        private readonly MemoizedFunction<DMesh3, MeshProjectionTarget> MeshToProjectionTarget;

        public DMesh3 DMesh => GeometryToDMesh.Call(Source);
        public Reducer Reducer => DMeshToReducer.Call(DMesh);
        public DMeshAABBTree3 AABBTree => DMeshToAABBTree.Call(DMesh);
        public MeshProjectionTarget ProjectionTarget => MeshToProjectionTarget.Call(DMesh);

        public G3SharpGeometryAdapter()
        {
            MeshToProjectionTarget = new MemoizedFunction<DMesh3, MeshProjectionTarget>((m) => new MeshProjectionTarget(m, AABBTree));
        }

        public IGeometry Reduce(IGeometry g, int vertexCount, bool project = false)
        {
            if (Result != null && vertexCount > Result.Vertices.Count)
                DMeshToReducer.Reset();
            Source = g;
            var reducer = Reducer;

            reducer.SetExternalConstraints(new MeshConstraints());
            MeshConstraintUtil.FixAllBoundaryEdges(reducer.Constraints, DMesh);

            // reducer.ProjectionMode = Reducer.TargetProjectionMode.
            reducer.PreserveBoundaryShape = true;            
            if (project)
                reducer.SetProjectionTarget(ProjectionTarget);
            return Result = reducer.Reduce(vertexCount, true).ToIGeometry();
        }
    }
}
