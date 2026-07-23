using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPathHandler : MonoBehaviour
{
    public Transform transformRootObject;

    void OnDrawGizmos()
    {
        if (transformRootObject == null) return;

        // Получаем все дочерние объекты, которые имеют компонент WaypointNode
        WaypointNode[] waypointNodes = transformRootObject.GetComponentsInChildren<WaypointNode>();

        Gizmos.color = Color.green;  // Цвет линий

        // Проходим по всем WaypointNode
        foreach (WaypointNode waypointNode in waypointNodes)
        {
            // Если у этого WaypointNode есть следующие узлы (nextWaypointNode)
            if (waypointNode.nextWaypointNode != null)
            {
                // Рисуем линии между этим узлом и всеми его следующими узлами
                foreach (var nextNode in waypointNode.nextWaypointNode)
                {
                    // Проверяем, что следующий узел существует
                    if (nextNode != null)
                    {
                        // Рисуем линию от текущего узла к следующему узлу
                        Gizmos.DrawLine(waypointNode.transform.position, nextNode.transform.position);
                    }
                }
            }
        }
    }
}
