using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
public static class UtilsClass
{
    private static Camera mainCamera;

    public static Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.nearClipPlane + 1;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePos);
        mouseWorldPosition.y = 0f;

        return mouseWorldPosition;
    }

    /* public static void RotateToTarget(Transform transformToRotate, Transform target, float rotateSpeed = 0, float rotationAngleOffset = 0, bool forward = false)
     {
         Vector3 dirVector = target.position - transformToRotate.position;

         Quaternion targetRotation = Quaternion.LookRotation(forward? dirVector : -dirVector);
         Quaternion rotation = Quaternion.RotateTowards(transformToRotate.rotation, targetRotation, rotateSpeed > 0 ? rotateSpeed * Time.deltaTime : 10000);
         transformToRotate.SetPositionAndRotation(transformToRotate.position, rotation);
     }*/
    public static (bool success, Vector3 position) GetMouseWorldPosition(LayerMask layer)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return (false, Vector3.zero);

        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, layer))
        {
            // The Raycast hit something, return with the position.
            return (success: true, position: hitInfo.point);
        }
        else
        {
            // The Raycast did not hit anything.
            return (success: false, position: Vector3.zero);
        }
    }
    public static void RotateToTarget(Transform transformToRotate,
    Transform target,
    float rotateSpeed = 0,
    float rotationAngleOffset = 0,
    bool lookDirectlyAtTarget = true) // Renamed "forward" to clarify intent)
    {
        if (target == null) return;

        Vector3 directionToTarget = target.position - transformToRotate.position;
        if (directionToTarget.sqrMagnitude < 0.001f) return;

        // Calculate the target rotation
        Quaternion targetRotation;

        if (lookDirectlyAtTarget)
        {
            // Standard LookAt (forward faces target)
            targetRotation = Quaternion.LookRotation(directionToTarget);
        }
        else
        {
            // Rotate around target (Y-axis only, no Z-axis tilt)
            directionToTarget.y = 0; // Flatten to XZ plane (optional, removes vertical influence)
            targetRotation = Quaternion.LookRotation(directionToTarget);
        }

        // Apply angle offset (e.g., for facing slightly left/right of target)
        if (rotationAngleOffset != 0)
        {
            targetRotation *= Quaternion.Euler(0, rotationAngleOffset, 0);
        }

        // Apply rotation
        if (rotateSpeed > 0)
        {
            transformToRotate.rotation = Quaternion.RotateTowards(
                transformToRotate.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime);
        }
        else
        {
            transformToRotate.rotation = targetRotation;
        }
    }
    public static void RotateToTargetWithRigidbody(Rigidbody2D rigidbodyToRotate, Transform target, float rotateSpeed = 0, float rotationAngleOffset = 0, bool forward = false)
    {
        Vector2 dirVector = (Vector2)target.position - rigidbodyToRotate.position;

        Quaternion targetRotation = Quaternion.LookRotation(rigidbodyToRotate.transform.forward, forward ? dirVector : -dirVector);

        Quaternion rotation = Quaternion.RotateTowards(rigidbodyToRotate.transform.rotation, targetRotation * Quaternion.Euler(0, 0, rotationAngleOffset), rotateSpeed > 0 ? rotateSpeed * Time.deltaTime : 10000);
        rigidbodyToRotate.MoveRotation(rotation);

    }
    public static bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    public static (bool, UnityEngine.Component) RaycastToCheckObjectType(Type type)
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        UnityEngine.Component hitComponent = null;
        return ((hit.collider != null && hit.collider.TryGetComponent(type, out hitComponent)), hitComponent);

    }
    public static IEnumerator Wait(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
    public static Vector3 GetRandomDir()
    {
        return new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
    }
    public static int GetRandomSign()
    {
        return UnityEngine.Random.value < 0.5f ? 1 : -1;
    }


    public static string GetEnumDescription(Enum enumVal)
    {
        System.Reflection.MemberInfo[] memInfo = enumVal.GetType().GetMember(enumVal.ToString());
        DescriptionAttribute attribute = CustomAttributeExtensions.GetCustomAttribute<DescriptionAttribute>(memInfo[0]);
        return attribute.Description;
    }
    public static string GetDescription<T>(FieldInfo fieldName)
    {
        string result;
        if (fieldName != null)
        {
            try
            {
                object[] descriptionAttrs = fieldName.GetCustomAttributes(typeof(DescriptionAttribute), false);
                DescriptionAttribute description = (DescriptionAttribute)descriptionAttrs[0];
                result = (description.Description);
            }
            catch
            {
                result = null;
            }
        }
        else
        {
            result = null;
        }

        return result;
    }


}



