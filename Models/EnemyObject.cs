using System.Formats.Asn1;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp.Processing;
using TheAdventure;

namespace TheAdventure.Models;

public class EnemyObject : RenderableGameObject
{
    public enum EnemyState{
        None = 0,
        Idle,
        Walk,
        Attack,
        Defeated,
        TakingDamage
    }

    private int minDistanceToPlayer = 32;
    private double timeSinceLastAttack = 0.0;
    private bool is_flipped {get; set;} = false;

    public EnemyState State{ get; private set; }

    public EnemyObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SetState(EnemyState.Idle, is_flipped);
    }

    public void SetState(EnemyState state, bool flipped)
    {
        if(SpriteSheet.ActiveAnimation is not null){
            if (flipped){
                SpriteSheet.ActiveAnimation.Flip = RendererFlip.Horizontal;
            }
            else{
                SpriteSheet.ActiveAnimation.Flip = RendererFlip.None;
            }
        }
        
        if(State == EnemyState.Defeated) return;
        if(State == state){
            return;
        }
        else if (state == EnemyState.Attack){
            SpriteSheet.ActivateAnimation("Attack");
        }
        else if(state == EnemyState.Idle){
            SpriteSheet.ActivateAnimation("Idle");
        }
        else{
            var animationName = Enum.GetName<EnemyState>(state);
            SpriteSheet.ActivateAnimation(animationName);
        }

        if(SpriteSheet.ActiveAnimation is not null){
            if (flipped){
                SpriteSheet.ActiveAnimation.Flip = RendererFlip.Horizontal;
            }
            else{
                SpriteSheet.ActiveAnimation.Flip = RendererFlip.None;
            }
        }
        
        

        is_flipped = flipped;
        State = state;
    }

    public void UpdateEnemyPosition(int player_x, int player_y, double time)
    {
        if(State == EnemyState.Defeated) return;
        if(State == EnemyState.Attack) return;

        int move_x = 0;
        int move_y = 0;
        var flipped = false;
        if(Position.X - player_x > minDistanceToPlayer){
            flipped = true;
            move_x = -1;
        }
        else if(Position.X - player_x < -minDistanceToPlayer){
            move_x = +1;
        }
        
        if(Position.Y - player_y > minDistanceToPlayer){
            move_y = -1;
        }
        else if(Position.Y - player_y < -minDistanceToPlayer){
            move_y = +1;
        }

        if(move_x != 0 || move_y != 0){
            SetState(EnemyState.Walk, flipped);
        }
        else{
            SetState(EnemyState.Idle, flipped);
        }
        Position = (Position.X + move_x, Position.Y + move_y);
    }

    public void AttemptAttack(int direction_x, int direction_y, double time){
        bool flipped;
        if(Position.X - direction_x > 0){
            flipped = true;
        }
        else{
            flipped = false;
        }
        if(timeSinceLastAttack >= 2.5 && State == EnemyState.Idle)
        {
            
            EnemyState state;
                state = EnemyState.Attack;
                timeSinceLastAttack = 0.0;
            SetState(state, flipped);
        }
        else if(timeSinceLastAttack >= 2.5 && State == EnemyState.Attack){
            SetState(EnemyState.Walk, flipped);
        }
        else{
            timeSinceLastAttack += time;
        }
    }

    public bool AttemptDefeat(int direction_x, int direction_y, bool isPlayerAttacking, PlayerObject.PlayerStateDirection attackDirection){
        if(isPlayerAttacking && Math.Abs(Position.X - direction_x) <= minDistanceToPlayer && Math.Abs(Position.Y - direction_y) <= minDistanceToPlayer){
            bool isDefeated = false;
            bool flipped = false;
            if(Position.X - direction_x > 0 && attackDirection == PlayerObject.PlayerStateDirection.Right){
                isDefeated = true;
            }

            if(Position.X - direction_x < 0 && attackDirection == PlayerObject.PlayerStateDirection.Left){
                isDefeated = true;
            }
            
            if(Position.Y - direction_y > 0 && attackDirection == PlayerObject.PlayerStateDirection.Down){
                isDefeated = true;
            }

            if(Position.Y - direction_y < 0 && attackDirection == PlayerObject.PlayerStateDirection.Up){
                isDefeated = true;
            }

            if(isDefeated){
                return true;
            }
            return false;
        }
        return false;
    } 
}