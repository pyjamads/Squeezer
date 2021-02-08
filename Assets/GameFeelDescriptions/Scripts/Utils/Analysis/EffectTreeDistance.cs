using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFeelDescriptions
{
	/*
	 From https://arxiv.org/pdf/1805.06869.pdf
	 
function tree-edit-distance(Two input trees x¯ and y¯, a cost function c.)
	m ← |x¯|, n ← |y¯|.
	d ← m × n matrix of zeros. ⊲ di,j = dc(¯xi, y¯j ).
	D ← (m + 1) × (n + 1) matrix of zeros.
	⊲ Di,j = Dc(X[i, rlx¯(k)], Y [j, rly¯(l)]).
	for k ← m, . . . , 1 do
		for l ← n, . . . , 1 do
			DrlX(k)+1,rlY (l)+1 ← 0. ⊲ Equation 20
			for i ← rlX(k), . . . , k do
				Di,rlY (l)+1 ← Di+1,rlY (l)+1 + c(xi, −). ⊲ Equation 21
			end for
			for j ← rlY (l), . . . , l do
				DrlX(k)+1,j ← DrlX(k)+1,j+1 + c(−, yj). ⊲ Equation 22
			end for
			for i ← rlX(k), . . . , k do
				for j ← rlY (l), . . . , l do
					if rlx¯(i) = rlx¯(k) ∧ rly¯(j) = rly¯(l) then
						Di,j ← min{Di+1,j + c(xi, −),
						Di,j+1 + c(−, yj ),
						Di+1,j+1 + c(xi, yj)}. ⊲ Equation 16
						di,j ← Di,j .
					else
						Di,j ← min{Di+1,j + c(xi, −),
						Di,j+1 + c(−, yj ),
						Drlx¯(i)+1,rly¯(j)+1 + di,j}. ⊲ Equation 15
					end if
				end for
			end for
		end for
	end for
	return d1,1.
end function
	 */

	public class EffectTreeDistance
	{
		//function tree-edit-distance(Two input trees x¯ and y¯, a cost function c.)
		public static float TreeEditDistance(List<GameFeelEffect> X, List<GameFeelEffect> Y, bool debugActions = false)
		{
			//NOTE: This string sorting orders the sub-trees in a consistent manner,
			//NOTE: giving two similar sub-trees a higher likelihood of matching correctly 
			string generateSortString(List<GameFeelEffect> effects)
			{
				var result = "";
				foreach (var effect in effects.OrderBy(gameFeelEffect =>
					gameFeelEffect.GetType().Name + generateSortString(gameFeelEffect.ExecuteAfterCompletion) +
					(gameFeelEffect is SpawningGameFeelEffect spawner ? generateSortString(spawner.ExecuteOnOffspring) : "")))
				{
					result += effect.GetType().Name;
				}

				return result;
			}
			
			//Flattens the hierarchy of a list of effects into a single list
			List<GameFeelEffect> flatten(List<GameFeelEffect> effects)
			{
				var fullList = new List<GameFeelEffect>();

				foreach (var effect in effects.OrderBy(gameFeelEffect => generateSortString(gameFeelEffect.ExecuteAfterCompletion)))
				{
					fullList.Add(effect);
					
					fullList.AddRange(flatten(effect.ExecuteAfterCompletion));

					if (effect is SpawningGameFeelEffect spawner)
					{
						fullList.AddRange(flatten(spawner.ExecuteOnOffspring));
					}
				}

				return fullList;
			}
			
			//Flattens the hierarchy of a single effect into a single list
			List<GameFeelEffect> flattenSingle(GameFeelEffect effect)
			{
				var fullList = new List<GameFeelEffect>();
				
				//NOTE: maybe adds itself as the first item of the list
				//fullList.Add(effect);
				
				fullList.AddRange(flatten(effect.ExecuteAfterCompletion));

				if (effect is SpawningGameFeelEffect spawner)
				{
					fullList.AddRange(flatten(spawner.ExecuteOnOffspring));
				}

				return fullList;
			}
			
			//Get's the index of the right most leaf
			int rightLeaf(List<GameFeelEffect> effects, int index = 0)
			{
				//do this for everything but the root node.
				if (index > 0)
				{
					var list = flattenSingle(effects[index]);
				
					return index + list.Count;
				}

				//For the root node, the right most leaf, is the last item in the list.
				return effects.Count - 1;
			}
			
			var X_ = flatten(X);
			var Y_ = flatten(Y);
			
			//Insert roots
			X_.Insert(0, null);
			Y_.Insert(0, null);
			
			// m ← |x¯|, n ← |y¯|.
			var m = X_.Count;
			var n = Y_.Count;
			// d ← m × n matrix of zeros. // ⊲ di,j = dc(¯xi, y¯j ). //the cost of each
			var d = new float[m,n];
			var dActions = new string[m, n];
			// D ← (m + 1) × (n + 1) matrix of zeros. //⊲ Di,j = Dc(X[i, rlx¯(k)], Y [j, rly¯(l)]).
			var D = new float[m + 1, n + 1];
			var DActions = new string[m +1, n + 1];
			
			// 	 for k ← m, . . . , 1 do
			for (int k = m - 1; k >= 0; k--) //NOTE: using m-1 .. 0 instead of m ... 1
			{
				// 	for l ← n, . . . , 1 do
				for (int l = n - 1; l >= 0; l--) //NOTE: using n-1 .. 0 instead of n ... 1
				{
					var rlX_k = rightLeaf(X_, k);
					var rlY_l = rightLeaf(Y_, l);

					// 	DrlX(k)+1,rlY (l)+1 ← 0. ⊲ Equation 20
					D[rlX_k + 1, rlY_l + 1] = 0;
					if (debugActions)
					{
						DActions[rlX_k + 1, rlY_l + 1] = "";
					}

					// 	for i ← rlX(k), . . . , k do
					for (int i = rlX_k; i >= k; i--)
					{
						// 	Di,rlY (l)+1 ← Di+1,rlY (l)+1 + c(xi, −). ⊲ Equation 21
						D[i, rlY_l + 1] = D[i + 1, rlY_l + 1] + GameFeelEffect.ReplacementCost(X_[i], null);// + 0.01f; //c(xi, -) means delete
						if (debugActions)
						{
							DActions[i, rlY_l + 1] = DActions[i + 1, rlY_l + 1] + (X_[i] == null ? "" : "Delete("+X_[i]?.GetType().Name+")\n");
						}
					}
					// 	end for

					// 	for j ← rlY (l), . . . , l do
					for (int j = rlY_l; j >= l; j--)
					{
						// 	DrlX(k)+1,j ← DrlX(k)+1,j+1 + c(−, yj). ⊲ Equation 22
						D[rlX_k + 1, j] = D[rlX_k + 1, j + 1] + GameFeelEffect.ReplacementCost(null, Y_[j]);// + 0.0001f; //c(−, yj) means insert
						if (debugActions)
						{
							DActions[rlX_k + 1, j] = DActions[rlX_k + 1, j + 1] + (Y_[j] == null ? "" : "Insert("+Y_[j]?.GetType().Name+")\n");
						}
					}
					// 		end for
					
					// 		for i ← rlX(k), . . . , k do
					for (int i = rlX_k; i >= k; i--)
					{
						var rlx_i = rightLeaf(X_, i);
						
						// 	for j ← rlY (l), . . . , l do
						for (int j = rlY_l; j >= l; j--)
						{
							var rly_j = rightLeaf(Y_, j);
							// 	if rlx¯(i) = rlx¯(k) ∧ rly¯(j) = rly¯(l) then
							if (rlx_i == rlX_k &&
							    rly_j == rlY_l)
							{
								// // 	Di,j ← min{
								// D[i, j] = Mathf.Min(
								// 	// 	Di+1,j + c(xi, −),
								// 	D[i+1, j] + GameFeelEffect.ReplacementCost(X_[i], null) + 0.01f,
								// 	// 	Di,j+1 + c(−, yj ),
								// 	D[i, j+1] + GameFeelEffect.ReplacementCost(null, Y_[j]) + 0.0001f,
								// 	// 	Di+1,j+1 + c(xi, yj)
								// 	D[i+1,j+1] + GameFeelEffect.ReplacementCost(X_[i], Y_[j]) + 0.000001f // c(xi, yj) means replace
								// // 	}. ⊲ Equation 16
								// );
								
								// 	Di,j ← min{
								// 	Di+1,j + c(xi, −),
								D[i, j] = D[i + 1, j] + GameFeelEffect.ReplacementCost(X_[i], null);// + 0.01f;
								if (debugActions)
								{
									DActions[i, j] = DActions[i + 1, j] + (X_[i] == null ? "" : "Delete("+X_[i]?.GetType().Name+")\n");
								}

								// 	Di,j+1 + c(−, yj ),
								var val = D[i, j + 1] + GameFeelEffect.ReplacementCost(null, Y_[j]);// + 0.0001f;
								if (val < D[i, j])
								{
									D[i, j] = val;
									if (debugActions)
									{
										DActions[i, j] = DActions[i, j + 1] + (Y_[j] == null ? "" : "Insert("+Y_[j]?.GetType().Name+")\n");
									}
								}
								
								// 	Di+1,j+1 + c(xi, yj)
								val = D[i + 1, j + 1] + GameFeelEffect.ReplacementCost(X_[i], Y_[j]);// + 0.000001f; // c(xi, yj) means replace 
								if (val < D[i, j])
								{
									D[i, j] = val;
									if (X_[i] == null && Y_[j] == null || GameFeelEffect.ReplacementCost(X_[i], Y_[j]) == 0)
									{
										if (debugActions)
										{
											DActions[i, j] = DActions[i + 1, j + 1];
										}
									}
									else
									{
										if (debugActions)
										{
											DActions[i, j] = DActions[i + 1, j + 1] + "Replace("+X_[i]?.GetType().Name+"->"+Y_[j]?.GetType().Name+")=["+GameFeelEffect.ReplacementCost(X_[i], Y_[j]).ToString("F")+"]\n";
										}	
									}
								}
								// 	}. ⊲ Equation 16

								// 	di,j ← Di,j.	
								d[i, j] = D[i, j];
								if (debugActions)
								{
									dActions[i, j] = DActions[i, j];
								}
							}
							// 	else
							else
							{
								// // 	Di,j ← min{
								// D[i, j] = Mathf.Min(
								// 	// 	Di+1,j + c(xi, −),
								// 	D[i+1,j] + GameFeelEffect.ReplacementCost(X_[i], null) + 0.01f,
								// 	// 	Di,j+1 + c(−, yj ),
								// 	D[i, j+1] + GameFeelEffect.ReplacementCost(null, Y_[j]) + 0.0001f,
								// 	// 	Drlx¯(i)+1,rly¯(j)+1 + di,j
								// 	D[rlx_i+1, rly_j+1] + d[i,j]
								// // 	}. ⊲ Equation 15
								// );
								
								// 	Di,j ← min{
								// 	Di+1,j + c(xi, −),
								D[i, j] = D[i + 1, j] + GameFeelEffect.ReplacementCost(X_[i], null);// + 0.01f;
								if (debugActions)
								{
									DActions[i, j] = DActions[i + 1, j] + (X_[i] == null ? "" : "Delete("+X_[i]?.GetType().Name+")\n");
								}
								
								// 	Di,j+1 + c(−, yj ),
								var val = D[i, j + 1] + GameFeelEffect.ReplacementCost(null, Y_[j]);// + 0.0001f;
								if (val < D[i, j])
								{
									D[i, j] = val;
									if (debugActions)
									{
										DActions[i, j] = DActions[i, j + 1] + (Y_[j] == null ? "" : "Insert("+Y_[j]?.GetType().Name+")\n");
									}
								}

								val = D[rlx_i + 1, rly_j + 1] + d[i, j];
								if (val < D[i, j])
								{
									D[i, j] = val;
									if (debugActions)
									{
										DActions[i, j] = DActions[rlx_i + 1, rly_j + 1] + dActions[i,j];
									}
								}
							}
							// 	end if
						}
						// 	end for
					}
					// 	end for
				}
				// 	end for	
			}
			// end for
		
			// return d1,1.
			if (debugActions)
			{
				Debug.Log("Actions: "+dActions[0,0]);
			}
			return d[0,0];
		}
	}
}