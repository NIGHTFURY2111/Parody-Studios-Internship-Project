public class StateFactory
{
    PlayerStateMachine _context;

    IdleState idleState;
    JumpState jumpState;
    MoveState moveState;
    FallState fallState;
    public StateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        idleState = new IdleState(_context,this);
        jumpState = new JumpState(_context,this);
        moveState = new MoveState(_context,this);
        fallState = new FallState(_context,this);

    }

    public BaseState Idle() 
    {
        return idleState;
    }
    public BaseState Jump() 
    {
        return jumpState;
    }
    public BaseState Move() 
    {
        return moveState;
    }
    public BaseState Fall() 
    {
        return fallState;
    }
    //DO THE SAME FOR ALL CLASSES
}

[System.Flags]
public enum StateEnum
{
    move = 1<<0,
    idle = 1<<1,
    jump = 1<<2,
    fall = 1<<3,
}