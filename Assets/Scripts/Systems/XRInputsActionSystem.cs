
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UI;
using Unity.Entities.UniversalDelegates;
using UnityEngine.UIElements;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Matrix4x4 = UnityEngine.Matrix4x4;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
using Unity.Physics.Extensions;
using UnityEngine.InputSystem;
using Unity.Rendering;
using static UnityEngine.InputSystem.InputAction;
using Vector2 = UnityEngine.Vector2;
using UnityEngine.Animations;
using static UnityEngine.EventSystems.EventTrigger;
using System;
using Unity.VisualScripting.FullSerializer;
using Unity.Collections;


///VRAIMENT PAS SUR DE CA 
[UpdateInGroup(typeof(InitializationSystemGroup))]

public partial class XRInputsActionSystem : SystemBase
{

    private EntityCommandBuffer ecb;
    private EntityCommandBuffer BSecb;

    private EntityManager entityManager;

    XRIDefaultInputActions inputs_action;

    //relatif au character controler du physicsSample
    EntityQuery m_CharacterControllerInputQuery;
    Vector2 m_CharacterMovement;
    EntityQuery headQuery;

    private MaterialChangerSystem Materialchangersystem;
    private ItemSystem Itemsystem;

    private EntityCommandBuffer commandBufferSystem;

    //XRDeviceSimulatorControls inputs_action_simulated;
    //Temp_test test_inputs;

    private InputAction right_hand_trigger;
    private InputAction left_hand_trigger;


    private InputAction right_hand_pad;
    //private InputAction right_hand_pad;

    private InputAction right_hand_grab;
    private InputAction left_hand_grab;

    private InputAction right_hand_trigger_value;
    private InputAction left_hand_trigger_value;


    private InputAction A_button_trigger;
    private InputAction B_button_trigger;


    //sert a tester sans la vr
    private float temppp;


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;

        inputs_action = new XRIDefaultInputActions();

        m_CharacterControllerInputQuery = GetEntityQuery(typeof(CharacterControllerInput));
        headQuery = GetEntityQuery(typeof(PlayerHeadTag),typeof(LocalToWorld));

        Materialchangersystem = World.GetExistingSystemManaged<MaterialChangerSystem>();
        Itemsystem = World.GetExistingSystemManaged<ItemSystem>();


        right_hand_pad = inputs_action.XRIRightHandLocomotion.Move;

        right_hand_trigger = inputs_action.XRIRightHandInteraction.ActivateValue;
        left_hand_trigger = inputs_action.XRILeftHandInteraction.ActivateValue;

        right_hand_grab = inputs_action.XRIRightHandInteraction.Grip;
        left_hand_grab = inputs_action.XRILeftHandInteraction.Grip;

        A_button_trigger = inputs_action.XRIRightHandInteraction.A;
        B_button_trigger = inputs_action.XRILeftHandInteraction.B;

    }

    protected override void OnStartRunning()
    {

        inputs_action.Enable();


        inputs_action.XRIRightHandLocomotion.Move.performed += R_pad_activate;

        inputs_action.XRIRightHandInteraction.Grip.performed += R_Grab_activate;

        inputs_action.XRILeftHandInteraction.Grip.performed += L_Grab_activate;
        //inputs_action.XRIRightHandInteraction.Grip.canceled += Grab_deactivate;

        inputs_action.XRIRightHandInteraction.Activate.performed += R_Trigger_activate;
        inputs_action.XRIRightHandInteraction.Activate.canceled += R_Trigger_deactivate;

        inputs_action.XRILeftHandInteraction.Activate.performed += L_Trigger_activate;
        inputs_action.XRILeftHandInteraction.Activate.canceled += L_Trigger_deactivate;

        inputs_action.XRIRightHandInteraction.ActivateValue.performed += R_Trigger_valued;
        inputs_action.XRILeftHandInteraction.ActivateValue.performed += L_Trigger_valued;
        inputs_action.XRIRightHandInteraction.ActivateValue.canceled += R_Trigger_canceled;
        inputs_action.XRILeftHandInteraction.ActivateValue.canceled += L_Trigger_canceled;


        inputs_action.XRIRightHandInteraction.A.started += A_button_pressed;
        inputs_action.XRILeftHandInteraction.B.started += B_button_pressed;


    }


    protected override void OnUpdate()
    {

        // character controller
        //Debug.Log(m_CharacterControllerInputQuery.CalculateEntityCount());
        if (m_CharacterControllerInputQuery.CalculateEntityCount() == 0)
            EntityManager.CreateEntity(typeof(CharacterControllerInput));


    }

    private void A_button_pressed(CallbackContext obj)
    {

        Debug.Log("A");


        //m_CharacterMovement = new Vector2(1,0);

        
                Entities
        .WithoutBurst()
        .WithAll<GrabDetectData>()
        .WithAll<RightHandTag>()
        .ForEach((ref Entity entity) =>
        {
            if (entityManager.GetComponentData<GrabDetectData>(entity).grabbing == true)
            {

                Entity item_entity = entityManager.GetComponentData<GrabDetectData>(entity).grabbed_item;

                ItemData item = entityManager.GetComponentData<ItemData>(item_entity);


                Debug.Log(item.item_type_id);

                if (item.item_type_id == 1 || item.item_type_id == 4)
                {


                    Debug.Log("ejectA");
                    //eject mag
                    if (entityManager.GetComponentData<FirearmData>(item_entity).loaded_magazine != Entity.Null)
                        _eject_mag(item_entity, entityManager.GetComponentData<FirearmData>(item_entity).loaded_magazine);

                }


            }
        }).Run();
        
    }

    private void B_button_pressed(CallbackContext obj)
    {

        Debug.Log("B");

        Entities
        .WithoutBurst()
        .WithAll<GrabDetectData>()
        .WithAll<LeftHandTag>()
            .ForEach((ref Entity entity) =>
            {
                if (entityManager.GetComponentData<GrabDetectData>(entity).grabbing == true)
                {

                    Entity item_entity = entityManager.GetComponentData<GrabDetectData>(entity).grabbed_item;

                    ItemData item = entityManager.GetComponentData<ItemData>(item_entity);

                    Debug.Log(item.item_type_id);

                    if (item.item_type_id == 1 || item.item_type_id == 4)
                    {

                        Debug.Log("ejectB");
                        if (entityManager.GetComponentData<FirearmData>(item_entity).loaded_magazine != Entity.Null)
                            _eject_mag(item_entity, entityManager.GetComponentData<FirearmData>(item_entity).loaded_magazine);

                    }
                }
            }).Run();

    }


    private void R_pad_activate(CallbackContext obj)
    {

        //Debug.Log(obj.ReadValue<Vector2>());
        ///a reactiver pour les tests en vr
        
        m_CharacterMovement = obj.ReadValue<Vector2>();
        
        /*
        if (m_CharacterControllerInputQuery.CalculateEntityCount() == 0)
            EntityManager.CreateEntity(typeof(CharacterControllerInput));
        */
        //se referer a mes paints logic_charControl_mouv pour comprendre

        //if necessaire que si en update?
        //if (headQuery.CalculateEntityCount() == 1)
        {


            float horiz_rot = ((Quaternion)entityManager.GetComponentData<LocalToWorld>(headQuery.ToEntityArray(Allocator.TempJob)[0]).Rotation).eulerAngles.y;

            float axe_A = ((math.abs(horiz_rot - 180)) - 90) / 180;
            float axe_B = ((math.abs(horiz_rot - 180 + (horiz_rot <= 90 ? +270 : -90))) - 90) / 180;

            float mouv_x = m_CharacterMovement.x * axe_A + m_CharacterMovement.y * axe_B;
            float mouv_y = m_CharacterMovement.y * axe_A + m_CharacterMovement.x * -axe_B;



            ///smooth out movement
            ///peux pas changer le speed ici
            //mouv_x = mouv_x * m_CharacterMovement.x;
            //mouv_y =  mouv_y * m_CharacterMovement.y;

            //float2 mouv = new float2(0.1f, 0) * ((math.abs(m_CharacterMovement.x) + math.abs(m_CharacterMovement.y))*0.5f);


            //Debug.Log(mouv);
            //Debug.Log(mouv_x);
            //Debug.Log(mouv_y);

            m_CharacterControllerInputQuery.SetSingleton(new CharacterControllerInput
            {
                Looking = new float2(0, 0),
                Speed = (math.abs(m_CharacterMovement.x) + math.abs(m_CharacterMovement.y)) * 0.5f,
                Movement = new float2(mouv_x,mouv_y),
                Jumped = 0//m_CharacterJumped ? 1 : 0,
            });
        }
        


    }


    private void L_Trigger_deactivate(CallbackContext obj)
    {
        Entities
            .WithoutBurst()
            .WithAll<GrabDetectData>()
            .WithAll<LeftHandTag>()
        .ForEach((ref Entity entity) =>
        {
            if (entityManager.GetComponentData<GrabDetectData>(entity).grabbing == false)
                grab_detect_switch(entity, false);
            else
                Itemsystem.item_trigger_release_action(entityManager.GetComponentData<GrabDetectData>(entity).grabbed_item);

        }).Run();




    }

    private void L_Trigger_activate(CallbackContext obj)
    {

        Entities
            .WithoutBurst()
            .WithAll<GrabDetectData>()
            .WithAll<LeftHandTag>()
        .ForEach((ref Entity entity) =>
        {
            if (entityManager.GetComponentData<GrabDetectData>(entity).grabbing == false)
                grab_detect_switch(entity, true);
            else
            {
                //Debug.Log(entityManager.GetComponentData<ItemData>(entityManager.GetComponentData<GrabDetectData>(entity).grabbed_item).item_type_id);
                Itemsystem.item_trigger_action(entityManager.GetComponentData<GrabDetectData>(entity).grabbed_item);
            }

        }).Run();


    }


    private void R_Trigger_deactivate(CallbackContext obj)
    {
        Entities
            .WithoutBurst()
            .WithAll<GrabDetectData>()
            .WithAll<RightHandTag>()
        .ForEach((ref Entity entity) =>
        {
            if (entityManager.GetComponentData<GrabDetectData>(entity).grabbing == false)
            {
                grab_detect_switch(entity, false);
            }
            else
            {
                Itemsystem.item_trigger_release_action(entityManager.GetComponentData<GrabDetectData>(entity).grabbed_item);
            }


        }).Run();
    }

    private void R_Trigger_activate(CallbackContext obj)
    {

        Entities
        .WithoutBurst()
        .WithAll<GrabDetectData>()
        .WithAll<RightHandTag>()
        .ForEach((ref Entity entity) =>
        {
            if (entityManager.GetComponentData<GrabDetectData>(entity).grabbing == false)
                grab_detect_switch(entity, true);
            else
            {
                        //Debug.Log(entityManager.GetComponentData<ItemData>(entityManager.GetComponentData<GrabDetectData>(entity).grabbed_item).item_type_id);
                        Itemsystem.item_trigger_action(entityManager.GetComponentData<GrabDetectData>(entity).grabbed_item);
            }

        }).Run();


    }

    private void R_Grab_activate(CallbackContext obj)
    {
        Entities
            .WithoutBurst()
            .WithAll<GrabDetectData>()
            .WithAll<RightHandTag>()
        .ForEach((ref Entity entity) =>
        {
            if (entityManager.GetComponentData<GrabDetectData>(entity).grabbing == true)
                drop_item(entity);

        }).Run();
    }
    private void L_Grab_activate(CallbackContext obj)
    {
        Entities
            .WithoutBurst()
            .WithAll<GrabDetectData>()
            .WithAll<LeftHandTag>()
        .ForEach((ref Entity entity) =>
        {
            if (entityManager.GetComponentData<GrabDetectData>(entity).grabbing == true)
                drop_item(entity);

        }).Run();
    }

    private void L_Trigger_valued(CallbackContext obj)
    {

        Entities.WithoutBurst()
                .ForEach((ref TransformAspect trans, in LocalToWorld ltw, in FantomeFingerData fantomedata) =>
                {

                    //chercher a faire avec des filtres dans le for each plutot que des condition chaque frame? plus opti ?


                    if (fantomedata.hand_ID == 1)
                    {

                        if (entityManager.GetComponentData<GrabDetectData>(fantomedata.hand_root).grabbing == false)
                        {
                            //Debug.Log(left_hand_trigger.ReadValue<float>());
                            trans.LocalRotation = Quaternion.Slerp(Quaternion.Euler(fantomedata.open_hand_rot), Quaternion.Euler(fantomedata.close_hand_rot), left_hand_trigger.ReadValue<float>()*0.8f/*ferme a 80%*/);
                        }
                        //ici faire une rot pour chaque item?
                        else
                        {

                            switch (entityManager.GetComponentData<GrabDetectData>(fantomedata.hand_root).snap_type)
                            {
                                case 0: //null
                                    trans.LocalRotation = Quaternion.Euler(fantomedata.close_hand_rot);
                                    break;
                                case 1: // USP
                                    trans.LocalRotation = Quaternion.Slerp(Quaternion.Euler(fantomedata.hold_USP_0_hand_rot), Quaternion.Euler(fantomedata.hold_USP_1_hand_rot), left_hand_trigger.ReadValue<float>()); ;
                                    break;
                                    // mettre a la suite pour de nv objets
                            }

                        }
                    }


                }).Run();


    }


    private void R_Trigger_valued(CallbackContext obj)
    {

        //Debug.Log(right_hand_trigger.ReadValue<float>());

        Entities.WithoutBurst()
                .ForEach((ref TransformAspect trans, in LocalToWorld ltw, in FantomeFingerData fantomedata) =>
                {

                    //chercher a faire avec des filtres dans le for each plutot que des condition chaque frame? plus opti ?


                    if (fantomedata.hand_ID == 0)
                    {
                        if (entityManager.GetComponentData<GrabDetectData>(fantomedata.hand_root).grabbing == false)
                        {

                            Vector3 right_open = new Vector3(fantomedata.open_hand_rot.x, -fantomedata.open_hand_rot.y, -fantomedata.open_hand_rot.z);
                            Vector3 right_close = new Vector3(fantomedata.close_hand_rot.x, -fantomedata.close_hand_rot.y, -fantomedata.close_hand_rot.z);

                            trans.LocalRotation = Quaternion.Slerp(Quaternion.Euler(right_open), Quaternion.Euler(right_close), right_hand_trigger.ReadValue<float>() * 0.8f/*ferme a 80%*/);
                        }
                        //ici faire une rot pour chaque item?
                        else
                        {

                            switch (entityManager.GetComponentData<GrabDetectData>(fantomedata.hand_root).snap_type)
                            {
                                case 0: //null
                                    Vector3 right_close = new Vector3(fantomedata.close_hand_rot.x, -fantomedata.close_hand_rot.y, -fantomedata.close_hand_rot.z);
                                    trans.LocalRotation = Quaternion.Euler(right_close);
                                    break;
                                case 1: // USP
                                    Vector3 right_usp_0 = new Vector3(fantomedata.hold_USP_0_hand_rot.x, -fantomedata.hold_USP_0_hand_rot.y, -fantomedata.hold_USP_0_hand_rot.z);
                                    Vector3 right_usp_1 = new Vector3(fantomedata.hold_USP_1_hand_rot.x, -fantomedata.hold_USP_1_hand_rot.y, -fantomedata.hold_USP_1_hand_rot.z);
                                    trans.LocalRotation = Quaternion.Slerp(Quaternion.Euler(right_usp_0), Quaternion.Euler(right_usp_1), right_hand_trigger.ReadValue<float>()); ;
                                    break;
                                // mettre a la suite pour de nv objets
                            }

                        }
                    }


                }).Run();
    }

    private void L_Trigger_canceled(CallbackContext obj)
    {

        Entities.WithoutBurst()
                .ForEach((ref TransformAspect trans, in LocalToWorld ltw, in FantomeFingerData fantomedata) =>
                {

                    //chercher a faire avec des filtres dans le for each plutot que des condition chaque frame? plus opti ?


                    if (fantomedata.hand_ID == 1)
                    {
                        if (entityManager.GetComponentData<GrabDetectData>(fantomedata.hand_root).grabbing == false)
                        {

                            Vector3 left_open = new Vector3(fantomedata.open_hand_rot.x, fantomedata.open_hand_rot.y, fantomedata.open_hand_rot.z);
                            Vector3 left_close = new Vector3(fantomedata.close_hand_rot.x, fantomedata.close_hand_rot.y, fantomedata.close_hand_rot.z);

                            trans.LocalRotation = Quaternion.Slerp(Quaternion.Euler(left_open), Quaternion.Euler(left_close), 0);
                        }

                    }


                }).Run();


    }
    private void R_Trigger_canceled(CallbackContext obj)
    {

        Entities.WithoutBurst()
                .ForEach((ref TransformAspect trans, in LocalToWorld ltw, in FantomeFingerData fantomedata) =>
                {

                    //chercher a faire avec des filtres dans le for each plutot que des condition chaque frame? plus opti ?


                    if (fantomedata.hand_ID == 0)
                    {
                        if (entityManager.GetComponentData<GrabDetectData>(fantomedata.hand_root).grabbing == false)
                        {
                            Vector3 right_open = new Vector3(fantomedata.open_hand_rot.x, -fantomedata.open_hand_rot.y, -fantomedata.open_hand_rot.z);
                            Vector3 right_close = new Vector3(fantomedata.close_hand_rot.x, -fantomedata.close_hand_rot.y, -fantomedata.close_hand_rot.z);

                            trans.LocalRotation = Quaternion.Slerp(Quaternion.Euler(right_open), Quaternion.Euler(right_close), 0);
                        }

                    }


                }).Run();


    }





    protected override void OnStopRunning()
    {
        inputs_action.Disable();
        inputs_action.XRIRightHandInteraction.Grip.Disable();
    }


    public void grab_detect_switch(Entity hand, bool state)
    {

        commandBufferSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        if (state == true)
        {
            commandBufferSystem.AddComponent<GrabDetectTag>(hand);
        }
        else
        {
            commandBufferSystem.RemoveComponent<GrabDetectTag>(hand);
        }

    }



    public void drop_item(Entity holding_hand)
    {

        PhysicsMass new_mass = entityManager.GetComponentData<PhysicsMass>(holding_hand);

        commandBufferSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Entity item_entity = entityManager.GetComponentData<GrabDetectData>(holding_hand).grabbed_item;

        ItemData item = entityManager.GetComponentData<ItemData>(item_entity);

        GrabDetectData new_grabdetect = entityManager.GetComponentData<GrabDetectData>(holding_hand);


        if (item.snap_type != 0)
        {


            Materialchangersystem.change_material(1, entityManager.HasComponent<RightHandTag>(holding_hand) ? 0 : 1);


            if (entityManager.HasComponent<RightHandTag>(holding_hand))
            {


                Entities
                    .WithoutBurst()
                    .WithAll<Disabled>()
                    .WithAll<RightHandTag>()
                .ForEach((Entity entity, in AvatarFingerData fingerdata) =>
                {

                    var trans = SystemAPI.GetAspectRW<TransformAspect>(entity);

                    trans.WorldPosition = SystemAPI.GetAspectRO<TransformAspect>(fingerdata.fantome_correspondant).WorldPosition;
                    trans.WorldRotation = SystemAPI.GetAspectRO<TransformAspect>(fingerdata.fantome_correspondant).WorldRotation;

                    //y faut pas que ca disable les childs du finger
                    commandBufferSystem.RemoveComponent<Disabled>(entity);


                    //je recupere l entity_tracked pour le skin et je le depalce sur le doigt fantome
                    commandBufferSystem.AddComponent(fingerdata.Skin_finger_entity, new Parent { Value = entity });

                }).Run();

            }
            else if (entityManager.HasComponent<LeftHandTag>(holding_hand))
            {


                Entities
                    .WithoutBurst()
                    .WithAll<Disabled>()
                    .WithAll<LeftHandTag>()
                .ForEach((Entity entity, in AvatarFingerData fingerdata) =>
                {

                    var trans = SystemAPI.GetAspectRW<TransformAspect>(entity);

                    trans.WorldPosition = SystemAPI.GetAspectRO<TransformAspect>(fingerdata.fantome_correspondant).WorldPosition;
                    trans.WorldRotation = SystemAPI.GetAspectRO<TransformAspect>(fingerdata.fantome_correspondant).WorldRotation;

                    //y faut pas que ca disable les childs du finger
                    commandBufferSystem.RemoveComponent<Disabled>(entity);


                    //je recupere l entity_tracked pour le skin et je le depalce sur le doigt fantome
                    commandBufferSystem.AddComponent(fingerdata.Skin_finger_entity, new Parent { Value = entity });

                }).Run();

            }


        }



        if (item.hand_grabbing == 3)
        {
            if (entityManager.HasComponent<RightHandTag>(holding_hand))
                item.hand_grabbing = 2;
            else
                item.hand_grabbing = 1;
        }
        else
        {
            item.hand_grabbing = 0;
        }

        new_grabdetect.grabbing = false;

        entityManager.SetComponentData<GrabDetectData>(holding_hand, new_grabdetect);

        item.grabbed = false;
        entityManager.SetComponentData<ItemData>(item_entity, item);
        //Debug.Log("drop")
        commandBufferSystem.DestroyEntity(entityManager.GetComponentData<GrabDetectData>(holding_hand).hand_grab_joint);
        
    }

    private void _eject_mag(Entity gun, Entity mag)
    {

        ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        BSecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Translation new_trans;

        FirearmData new_firearm_data;

        //A CHANGER MARCHE QUE QUAND CHARGER FULL
        switch (entityManager.GetComponentData<MagazineData>(mag).max_bullets_in_clip)
        {

            //USP
            case 10:

                //decale le mag pour pas etre rejoin tt suite

                new_trans = entityManager.GetComponentData<Translation>(mag);

                new_trans.Value -= (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(mag).Rotation * new Vector3(0, 0.06f, 0));

                ecb.SetComponent<Translation>(mag, new_trans);


                //DANGER plusieur tag possible ?
                ecb.AddComponent<GrabbableTag>(mag);

                new_firearm_data = entityManager.GetComponentData<FirearmData>(gun);

                new_firearm_data.loaded_magazine = Entity.Null;

                ecb.SetComponent<FirearmData>(gun, new_firearm_data);

                ecb.DestroyEntity(entityManager.GetComponentData<MagazineData>(mag).loaded_mag_joint);


                ecb.AddComponent<MagazineChamberTag>(gun);
                ecb.AddComponent<MagazineTag>(mag);

                // DISABLE LES COL AVEC LA SLIDE

                MagazineData mag_data = new MagazineData();

                mag_data = entityManager.GetComponentData<MagazineData>(mag);

                Entity slide_entity = entityManager.GetComponentData<ItemData>(gun).composite_sub_part;

                ComponentType[] col_joint_componentTypes =
        {
            typeof(PhysicsConstrainedBodyPair),
            typeof(PhysicsJoint)
            };

                var col_joint_constraints = new FixedList512Bytes<Constraint>();
                col_joint_constraints.Add(new Constraint
                {
                    ConstrainedAxes = new bool3(false),
                    Type = ConstraintType.Angular,
                    Min = -math.PI,
                    Max = math.PI,
                    SpringFrequency = Constraint.DefaultSpringFrequency,
                    SpringDamping = Constraint.DefaultSpringDamping
                });
                var col_joint = new PhysicsJoint()
                {
                    BodyAFromJoint = BodyFrame.Identity,
                    BodyBFromJoint = BodyFrame.Identity,
                };
                col_joint.SetConstraints(col_joint_constraints);

                Entity col_jointEntity = entityManager.CreateEntity(col_joint_componentTypes);

                entityManager.SetComponentData(col_jointEntity, new PhysicsConstrainedBodyPair(slide_entity, mag, false));

                ecb.AddSharedComponent(col_jointEntity, new PhysicsWorldIndex());

                mag_data.disable_col_joint = col_jointEntity;

                ecb.SetComponent<MagazineData>(mag, mag_data);



                float3 relative_pos = entityManager.GetComponentData<LocalToWorld>(mag).Position - entityManager.GetComponentData<LocalToWorld>(gun).Position;
                Quaternion relative_rot = Quaternion.Inverse((Quaternion)entityManager.GetComponentData<LocalToWorld>(gun).Rotation) * entityManager.GetComponentData<LocalToWorld>(mag).Rotation;


                PhysicsJoint snap_joint;
               

                var localFrame = new BodyFrame { Axis = new float3(0, -0.13f, 0.87f), PerpendicularAxis = new float3(0, 1, 0), Position = float3.zero };
                var worldFrame = new BodyFrame { Axis = new float3(0, -0.13f, 0.87f), PerpendicularAxis = new float3(0, 1, 0), Position = float3.zero };

                snap_joint = PhysicsJoint.CreateFixed(
                    localFrame,
                    worldFrame

                );



                ComponentType[] componentTypes =
                {
            typeof(PhysicsConstrainedBodyPair),
            typeof(PhysicsJoint)
            };

                var constraints = new FixedList512Bytes<Constraint>();


                constraints.Add(new Constraint
                {
                    ConstrainedAxes = new bool3(true, false, true),//slide,pull,starf
                    Type = ConstraintType.Linear,
                    Min = 0,
                    Max = 0,
                    SpringFrequency = 800000f,//74341.31f,
                    SpringDamping = 2530.126f,
                    MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                });



                constraints.Add(new Constraint
                {
                    ConstrainedAxes = new bool3(true, true, true), // ptich, roll, yaw
                    Type = ConstraintType.Angular,
                    Min = 0,
                    Max = 0,
                    SpringFrequency = 800000f,//74341.31f,
                    SpringDamping = 2530.126f,
                    MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                });

                snap_joint.SetConstraints(constraints);

                Entity jointEntity = entityManager.CreateEntity(componentTypes);

                entityManager.SetComponentData(jointEntity, new PhysicsConstrainedBodyPair(gun, mag, false));
                entityManager.SetComponentData(jointEntity, snap_joint);

                ecb.AddSharedComponent(jointEntity, new PhysicsWorldIndex());




                MagazineAttachmentData attach_data = new MagazineAttachmentData();

                attach_data.mag_joint_entity = jointEntity;
                attach_data.mag_entity = mag;
                attach_data.firearm_entity = gun;


                BSecb.AddComponent(mag, attach_data);
                break;

            //SKORPION
            case 25:

                //decale le mag pour pas etre rejoin tt suite

                new_trans = entityManager.GetComponentData<Translation>(mag);

                new_trans.Value -= (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(mag).Rotation * new Vector3(0, 0.06f, 0));


                ecb.SetComponent<Translation>(mag, new_trans);


                //DANGER plusieur tag possible ?
                ecb.AddComponent<GrabbableTag>(mag);

                new_firearm_data = entityManager.GetComponentData<FirearmData>(gun);

                new_firearm_data.loaded_magazine = Entity.Null;

                ecb.SetComponent<FirearmData>(gun, new_firearm_data);

                ecb.DestroyEntity(entityManager.GetComponentData<MagazineData>(mag).loaded_mag_joint);


                ecb.AddComponent<MagazineChamberTag>(gun);
                ecb.AddComponent<MagazineTag>(mag);
                ecb.AddComponent<StorableTag>(mag);


                break;

        }



    }

}
