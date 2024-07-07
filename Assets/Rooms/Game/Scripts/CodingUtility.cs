using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Game.ArrowState;

namespace Game
{
    public static class CodingUtility
    {
        public const float GRID_SIZE = 20;
        public const int MAX_ARROWS_PER_PATH = 7;

        public static CodingSubPath ToSubPath(this CodingPath codingPath)
        {
            return new CodingSubPath
            {
                StartDirection = codingPath.StartDirection,
                SuggestedPath = codingPath.SuggestedPath
            };
        }

        public static bool TrySplitPathForArrows(this CodingPath codingPath, List<ArrowState> pool, ref CodingSubPath[] result)
        {
            Vector3[] path = codingPath.SuggestedPath;

            // Maximum number of 2 long subpaths obtainable from a n long path = n - 1
            int maxSplitCount = path.Length - 1;
            int minSubPathCount = codingPath.MinSubPathCount;

            if (minSubPathCount > maxSplitCount)
            {
                Debug.LogError($"Cound not split a {path.Length} long path into {minSubPathCount}.");
                return false;
            }

            for (int count = minSubPathCount; count <= maxSplitCount; count++)
            {
                List<Vector3[]> split = SplitPath(path, count);
                CodingSubPath[] subPaths = ConvertToCodingSubPaths(split, codingPath.StartDirection);

                if (IsSolvableWithArrows(subPaths, pool))
                {
                    result = subPaths;
                    return true;
                }
            }

            Debug.LogError("Could not split CodingPath into solvable subpath");

            return false;
        }

        public static List<Vector3[]> SplitPath(Vector3[] path, int count)
        {
            List<Vector3[]> result = new();

            int length = path.Length / count;
            int modulo = path.Length % count;

            for (int i = 0, from = 0; i < count; i++)
            {
                int currentLength = length;

                if (i < count - 1) currentLength++; // Subpath i overlaps with subpath i + 1
                if (i > count - modulo) currentLength++; // Distributing the remainder starting from the end

                Vector3[] split = new Vector3[currentLength];

                Array.Copy(path, from, split, 0, currentLength);

                result.Add(split);

                // Next subpath starts from the last element
                from += currentLength - 1;
            }

            return result;
        }

        public static bool TryGetOptimalSolution(CodingSubPath subPath, List<ArrowState> pool, out List<ArrowState> solution)
        {
            solution = new();

            Vector3[] path = subPath.SuggestedPath;
            Vector3 lastPosition = subPath.Start.Value;
            Vector3Int lastDirection = subPath.StartDirection;

            for (int i = 1; i < path.Length; i++)
            {
                Vector3Int movement = WorldToGridSpace(path[i] - lastPosition);
                Vector3Int direction = MovementToGridDirection(movement);

                if (direction.magnitude > 1 || direction == Vector3Int.zero || direction.y > 0)
                {
                    Debug.LogError("Invalid movement " + movement);
                    return false;
                }

                if (i < path.Length - 1)
                {
                    Vector3Int nextMovement = WorldToGridSpace(path[i + 1] - lastPosition);

                    // If next direction is the same, we accumulate
                    // TODO clean up this loop
                    if (MovementToGridDirection(nextMovement) == direction)
                    {
                        continue;
                    }
                }


                if (!TryGetRotationArrows(direction, lastDirection, pool, ref solution)
                    || !TryGetTranslationArrows(movement, pool, ref solution))
                {
                    return false;
                }

                if (solution.Count > MAX_ARROWS_PER_PATH) return false;

                lastPosition = path[i];
                lastDirection = direction;
            }

            return true;
        }

        public static bool IsSolvableWithArrows(CodingSubPath[] subPaths, List<ArrowState> pool)
            => Array.TrueForAll(subPaths, path => TryGetOptimalSolution(path, pool, out _));


        public static bool TryGetRotationArrows(Vector3Int from, Vector3Int to, List<ArrowState> pool, ref List<ArrowState> list)
        {
            if (from == to) return true;

            bool hasLeftArrow = pool.Contains(LEFT);
            bool hasRightArrow = pool.Contains(RIGHT);

            if (!hasRightArrow && !hasRightArrow)
            {
                Debug.LogError("Rotation without rotation arrows");
                return false;
            }

            if (from == -to) // 180
            {
                ArrowState arrow = hasLeftArrow ? LEFT : RIGHT;

                list.Add(arrow);
                list.Add(arrow);
            }
            else // 90
            {
                float sign = Mathf.Sign(Vector3.SignedAngle(to, from, Vector3.up));

                ArrowState arrow, opposite;

                if (sign < 0)
                {
                    arrow = LEFT;
                    opposite = RIGHT;
                }
                else
                {
                    arrow = RIGHT;
                    opposite = LEFT;
                }

                if (pool.Contains(arrow))
                    list.Add(arrow);
                else if (pool.Contains(X3))
                    list.AddRange(new[] { opposite, X3 });
                else
                    list.AddRange(new[] { opposite, opposite, opposite });
            }

            return true;
        }

        public static bool TryGetTranslationArrows(Vector3Int movement, List<ArrowState> pool, ref List<ArrowState> list)
        {
            int movementLeft = (int)movement.magnitude;

            bool hasTriple = pool.Contains(TRIPLE_UP);
            bool hasDouble = pool.Contains(DOUBLE_UP);
            bool hasSingle = pool.Contains(UP);
            bool hasX4 = pool.Contains(X4);
            bool hasX3 = pool.Contains(X3);

            while (movementLeft > 0)
            {
                if (movementLeft >= 12 && hasTriple && hasX4)
                {
                    list.AddRange(new[] { TRIPLE_UP, X4 });
                    movementLeft -= 12;
                }
                else if (movementLeft >= 9 && hasTriple && hasX3)
                {
                    list.AddRange(new[] { TRIPLE_UP, X3 });
                    movementLeft -= 9;
                }
                else if (movementLeft >= 8 && hasDouble && hasX4)
                {
                    list.AddRange(new[] { DOUBLE_UP, X4 });
                    movementLeft -= 8;
                }
                else if (movementLeft >= 6 && (hasTriple || (hasDouble && hasX3)))
                {
                    if (hasTriple)
                        list.AddRange(new[] { TRIPLE_UP, TRIPLE_UP });
                    else
                        list.AddRange(new[] { DOUBLE_UP, X3 });

                    movementLeft -= 6;
                }
                else if (movementLeft >= 4 && (hasDouble || (hasTriple && hasSingle) || (hasX4 && hasSingle)))
                {
                    if (hasDouble)
                        list.AddRange(new[] { DOUBLE_UP, DOUBLE_UP });
                    else if (hasTriple)
                        list.AddRange(new[] { TRIPLE_UP, UP });
                    else
                        list.AddRange(new[] { UP, X4 });

                    movementLeft -= 4;
                }
                else if (movementLeft >= 3 && hasTriple)
                {
                    list.Add(TRIPLE_UP);
                    movementLeft -= 3;
                }
                else if (movementLeft >= 3 && hasSingle && (hasDouble || hasX3))
                {
                    list.Add(UP);
                    list.Add(hasDouble ? DOUBLE_UP : X3);

                    movementLeft -= 3;
                }
                else if (movementLeft >= 2 && hasDouble)
                {
                    list.Add(DOUBLE_UP);
                    movementLeft -= 2;
                }
                else if (hasSingle)
                {
                    for (int i = 0; i < movementLeft; i++)
                        list.Add(UP);

                    movementLeft = 0;
                }
                else
                {
                    Debug.LogError("Failed to move forward with current arrows");
                    return false;
                }
            }

            return true;
        }

        private static Vector3Int MovementToGridDirection(Vector3Int movement)
        {
            Vector3Int direction = movement;
            direction.Clamp(-Vector3Int.one, Vector3Int.one);
            return direction;
        }

        private static CodingSubPath[] ConvertToCodingSubPaths(List<Vector3[]> split, Vector3Int startDirection)
        {
            var result = new CodingSubPath[split.Count];

            for (int i = 0; i < split.Count; i++)
            {
                Vector3[] path = split[i];

                result[i] = new CodingSubPath
                {
                    StartDirection = startDirection,
                    SuggestedPath = path
                };

                int count = path.Length;
                Vector3 lastMovement = path[count - 1] - path[count - 2];
                startDirection = WorldToGridSpace(lastMovement);
            }

            return result;
        }

        public static Vector3Int WorldToGridSpace(Vector3 world) => new(
            Mathf.FloorToInt(world.x / GRID_SIZE),
            Mathf.FloorToInt(world.y / GRID_SIZE),
            Mathf.FloorToInt(world.z / GRID_SIZE));


        public static string DirectionToString(Vector3Int direction)
        {
            return direction == Vector3Int.forward ? "▲" :
                direction == Vector3Int.back ? "▼" :
                direction == Vector3Int.left ? "◀" :
                direction == Vector3Int.right ? "▶" : "";
        }

        public static string ArrowStatesToString(List<ArrowState> arrows)
        {
            var builder = new StringBuilder();

            foreach (ArrowState arrow in arrows)
            {
                builder.Append(arrow switch
                {
                    UP => "↑",
                    DOUBLE_UP => "↑↑",
                    TRIPLE_UP => "↑↑↑",
                    LEFT => "↺",
                    RIGHT => "↻",
                    X2 => "×2",
                    X3 => "×3",
                    X4 => "×4",
                    _ => ""
                });
            }

            return builder.ToString();
        }

        //TODO used by the editor, check if it can be merged with the runtime one
        public static List<ArrowState> CodingPathToArrowStates(CodingPath path)
        {
            return CodingSubPathToArrowState(path.ToSubPath());
        }

        public static List<ArrowState> CodingSubPathToArrowState(CodingSubPath path)
        {
            var arrows = new List<ArrowState>();

            Vector3Int lastMovement = path.StartDirection; //grid space
            Vector3 position = path.Start.Value;

            for (int step = 1; step < path.SuggestedPath.Length; step++)
            {
                Vector3Int movement = WorldToGridSpace(path.SuggestedPath[step] - position);

                var arrowMovement = StepToArrowStates(movement, lastMovement);

                arrows.AddRange(arrowMovement);
                lastMovement = movement;
                position = path.SuggestedPath[step];
            }

            return arrows;
        }

        //TODO used by the editor, check if it can be merged with the runtime one
        public static ArrowState[] StepToArrowStates(Vector3Int movement, Vector3Int lastMovement)
        {
            if (movement.magnitude > 1 || movement.magnitude == 0)
            {
                throw new Exception("Path cannot be followed only with ↑↺↻");
            }

            if (movement == lastMovement) // Go forward
            {
                return new[] { UP };
            }
            else if (movement == -lastMovement) // 180
            {
                return new[] { RIGHT, RIGHT, UP };
            }
            else if (movement == Vector3Int.forward)
            {
                if (lastMovement == Vector3Int.left)
                {
                    return new[] { RIGHT, UP };
                }
                else if (lastMovement == Vector3Int.right)
                {
                    return new[] { LEFT, UP };
                }
            }
            else if (movement == Vector3Int.back)
            {
                if (lastMovement == Vector3Int.left)
                {
                    return new[] { LEFT, UP };
                }
                else if (lastMovement == Vector3Int.right)
                {
                    return new[] { RIGHT, UP };
                }
            }
            else if (movement == Vector3Int.left)
            {
                if (lastMovement == Vector3Int.forward)
                {
                    return new[] { LEFT, UP };
                }
                else if (lastMovement == Vector3Int.back)
                {
                    return new[] { RIGHT, UP };
                }
            }
            else if (movement == Vector3Int.right)
            {
                if (lastMovement == Vector3Int.forward)
                {
                    return new[] { RIGHT, UP };
                }
                else if (lastMovement == Vector3Int.back)
                {
                    return new[] { LEFT, UP };
                }
            }

            throw new Exception("Invalid Movement");
        }
    }
}
