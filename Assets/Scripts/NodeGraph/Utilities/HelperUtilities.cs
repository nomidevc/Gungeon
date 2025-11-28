using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities 
{
    /// <summary>
    /// Empty string debug check.
    /// </summary>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.LogError($"{fieldName} is empty and must contains a value {thisObject.name.ToString()}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// List empty or contanins null debug check - return true if there is an error.
    /// </summary>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableToCheck)
    {
        if (enumerableToCheck == null)
        {
            Debug.LogError($"{fieldName} is null and must be assigned a value in {thisObject.name.ToString()}");
            return true;
        }
        int count = 0;
        foreach (var item in enumerableToCheck)
        {
            if (item == null)
            {
                Debug.LogError($"{fieldName} has null values and all values must be assigned in {thisObject.name.ToString()}");
                return true;
            }
            count++;
        }
        if (count == 0)
        {
            Debug.LogError($"{fieldName} is empty and must be assigned a value in {thisObject.name.ToString()}");
            return true;
        }
        return false;
    }
}
