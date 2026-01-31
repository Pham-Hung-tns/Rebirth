using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WanderAction : AIAction
{
    private Vector3 targetPosition;
    private EnemyMovement movement;

    public override void OnEnter()
    {
        enemyBrain.TimeLimit = enemyBrain.EnemyConfig.timeToWander;

        movement = enemyBrain != null ? enemyBrain.GetComponent<EnemyMovement>() : null;

        // Try to pick a random walkable tile from assigned groundTilemap and request path
        if (movement != null && movement.GroundTilemap != null)
        {
            Vector3? chosen = PickRandomWalkableCellWorld(movement.GroundTilemap, movement.CollisionTilemap);
            if (chosen.HasValue)
            {
                targetPosition = chosen.Value;
                enemyBrain.PatrolPosition = targetPosition;
                movement.RequestPath(targetPosition);
                return;
            }
        }
        
        enemyBrain.ChangeAnimationState(Settings.WANDER_STATE);
    }

    public override void OnUpdate()
    {
        // Nothing to manually move here: EnemyMovement will follow the requested path.
        // We can still update facing to face the target if needed.
        if (enemyBrain == null || enemyBrain.Rb == null)
            return;

        if (targetPosition != null)
            enemyBrain.ChangeDirection(targetPosition);
    }

    public override void OnExit()
    {
        enemyBrain.Rb.velocity = Vector2.zero;
    }


    // Pick a random walkable cell from the ground tilemap; avoid tiles that also exist on collisionTilemap
    private Vector3? PickRandomWalkableCellWorld(Tilemap ground, Tilemap collision)
    {
        if (ground == null) return null;
        BoundsInt bounds = ground.cellBounds;
        List<Vector3Int> candidates = new List<Vector3Int>();
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (!ground.HasTile(cell)) continue;
                if (collision != null && collision.HasTile(cell)) continue;
                candidates.Add(cell);
            }
        }

        if (candidates.Count == 0) return null;
        var chosen = candidates[Random.Range(0, candidates.Count)];
        return ground.GetCellCenterWorld(chosen);
    }
}
