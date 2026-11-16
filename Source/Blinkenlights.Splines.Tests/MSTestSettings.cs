// The tests only call the pure BSpline.Interpolate, so they can run in parallel.
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
