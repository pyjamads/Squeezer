// // using System;
// // using System.Linq;
// // using MathNet.Numerics.LinearAlgebra;
// // using MathNet.Numerics;
//
// using System.Numerics;
//
// namespace ClassicalMDS
// {
//     public class ClassicalMDS
//     {
//         private int n_components;
//
//         /// <summary>
//         /// Initialize a classical (Torgerson) Multidimensional Scaling instance.
//         /// 
//         /// A good introduction to Multidimensional Scaling (MDS) is given by:
//         /// 
//         /// http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.495.4629&rep=rep1&type=pdf
//         /// 
//         /// See section 2.1 for more details on Classical MDS.
//         /// </summary>
//         /// <param name="n_components">Dimension of the low-dimensional embedding.</param>
//         public ClassicalMDS(int n_components = 2)
//         {
//             this.n_components = n_components;
//         }
//
//         /// <summary>
//         /// Fits the given matrix X in R^(n x D) and returns the embedded coordinates as a matrix
//         /// Y in R^(n x L), where D is the dimension of the high-dimensional datapoints and L is the 
//         /// dimension of the low-dimenisional datapoints.
//         /// </summary>
//         /// <param name="X">The data matrix X as a matrix (n_samples. n_attributes) of
//         /// highdimensional points.</param>
//         /// <param name="dissimilarity">The mode with which to compute the distance matrix,
//         /// either "euclidean" for a euclidean distance metric, or "precomputed" to use X
//         /// as the distance matrix itself. If the mode is "precomputed", X needs to be a proper
//         /// symmetric distance matrix.</param>
//         /// <returns>The matrix of lowdimensional datapoints size (n_samples, n_components)</returns>
//         public Matrix<double> FitTransform(Matrix<double> X, String dissimilarity = "precomputed")
//         {
//             Matrix<double> D = Matrix<double>.Build.DenseOfMatrix(X);
//             if (dissimilarity == "euclidean")
//                 D = ComputeDistanceMatrix(X, "euclidean");
//             else if (dissimilarity == "precomputed")
//                 D = X;
//             else
//                 throw new MathNet.Numerics.InvalidParameterException();
//             if (D.RowCount != D.ColumnCount) // D is not a quadratic matrix
//                 throw new MathNet.Numerics.InvalidParameterException();
//             var n_datapoints = D.RowCount;
//             D = D.PointwisePower(2.0); // square the euclidean distance matrix
//             var E = Matrix<double>.Build.Dense(n_datapoints, 1, 1.0); // n x 1 matrix with ones
//             var H = Matrix<double>.Build.DenseIdentity(n_datapoints)
//                     - 1.0 / n_datapoints * (E * E.Transpose());
//             var B = (H * D * H).Multiply(-0.5); // inner product matrix
//             var evd = B.Evd();
//             // Sort for the highest eigenvalues
//             var evDiagonal = evd.D.Diagonal();
//             var evDiagSortedTuple = evDiagonal.EnumerateIndexed().OrderByDescending(tp => tp.Item2);
//             var permutationIndex = new Int32[evd.D.ColumnCount]; int i = 0;
//             foreach (var tuple in evDiagSortedTuple)
//             {
//                 permutationIndex[i] = tuple.Item1;
//                 i++;
//             }
//             evd.D.PermuteColumns(new Permutation(permutationIndex));
//             evd.EigenVectors.PermuteColumns(new Permutation(permutationIndex));
//             var Lambda = Matrix<double>.Build.DenseDiagonal(n_components, n_components,
//                 idx => evDiagSortedTuple.ElementAt(idx).Item2); // sub matrix of the n_-largest Eigenvalues
//             var V_p = evd.EigenVectors.SubMatrix(0, evd.EigenVectors.RowCount, 0, n_components); // sub matrix of the n_-largest Eigenvectors
//             //return Lambda.PointwisePower(0.5)*V_p.Transpose();
//             return V_p * (Lambda.PointwisePower(0.5));
//         }
//
//         /// <summary>
//         /// Computes the distance matrix (n_samples, n_samples) for the given matrix (n_samples, n_features)
//         /// with the specified metric. Only "euclidean" is supported.
//         /// </summary>
//         /// <param name="X">The matrix of data points (n_samples, n_features)</param>
//         /// <param name="metric">The distance metric to use, currently only "euclidean".</param>
//         /// <returns></returns>
//         private Matrix<double> ComputeDistanceMatrix(Matrix<double> X, string metric = "euclidean")
//         {
//             if (metric != "euclidean")
//                 throw new MathNet.Numerics.InvalidParameterException();
//             var D = Matrix<double>.Build.Dense(X.RowCount, X.RowCount); // n x n matrix with zeros
//             int i = 0; int j = 0;
//             foreach (Vector<double> row1 in X.EnumerateRows())
//             {
//                 foreach (Vector<double> row2 in X.EnumerateRows())
//                 {
//                     D[i, j] = Distance.Euclidean(row1, row2);
//                     j++;
//                 }
//                 j = 0;
//                 i++;
//             }
//             return D;
//         }
//     }
// }