using System.Numerics;
using BepuPhysics;

namespace LibreLancer.Physics;

internal class StaticObject : PhysicsObject
{
    public override bool Static => true;
    public override bool Active => false;
    public override Matrix4x4 Transform { get; protected set; }
    public override Vector3 Position { get; protected set; }

    public override bool Collidable
    {
        get => world.collidableObjects[BepuObject.Handle];
        set => world.collidableObjects[BepuObject.Handle] = value;
    }

    internal StaticReference BepuObject;
    private PhysicsWorld world;

    internal StaticObject(int id, StaticReference bepuObject, PhysicsWorld world, Matrix4x4 transform, Collider col) : base(id)
    {
        this.BepuObject = bepuObject;
        this.world = world;
        this.Collider = col;
        Transform = transform;
        Position = Vector3.Transform(Vector3.Zero, transform);
    }
    public override void SetTransform(Matrix4x4 transform)
    {
        Transform = transform;
        Position = Vector3.Transform(Vector3.Zero, transform);
        BepuObject.Pose = transform.ToPose();
        BepuObject.UpdateBounds();
    }

    public override Vector3 AngularVelocity
    {
        get => Vector3.Zero;
        set { /*Nop*/ }
    }

    public override Vector3 LinearVelocity
    {
        get => Vector3.Zero;
        set { /*Nop*/ }
    }
    public override BoundingBox GetBoundingBox()
    {
        var bounds = BepuObject.BoundingBox;
        return new BoundingBox(bounds.Min, bounds.Max);
    }

    public override Vector3 RotateVector(Vector3 src)
    {
        return Vector3.Transform(src, BepuObject.Pose.Orientation);
    }

    public override void SetDamping(float linearDamping, float angularDamping)
    {
        //Nop
    }

    public override void AddForce(Vector3 force)
    {
        //Nop
    }

    public override void Activate()
    {
        //Nop
    }

    public override void Impulse(Vector3 force)
    {
        //Nop
    }

    public override void AddTorque(Vector3 torque)
    {
        //Nop
    }

    public override void PredictionStep(float timestep)
    {
        //Nop
    }

    internal override void UpdateProperties()
    {
    }

    public override void Dispose()
    {
    }
}
