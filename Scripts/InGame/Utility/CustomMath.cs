using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMath
{
    //초점기준
    public static Vector2 GetEllipseCoord(float _x, float _y, float _degree)
    {
        float longAxis = _x > _y ? _x : _y;
        float shortAxis = _x > _y ? _y : _x;

        float x = shortAxis * shortAxis * Mathf.Cos(_degree * Mathf.Deg2Rad);
        float y = longAxis * longAxis * Mathf.Sin(_degree * Mathf.Deg2Rad);
        float divisor = longAxis + Mathf.Cos(_degree * Mathf.Deg2Rad) * Mathf.Sqrt(longAxis * longAxis - shortAxis * shortAxis);

        return new Vector2(x, y) / divisor;
    }

    //중심 기준
    public static Vector2 GetCenteredEllipseCoord(Vector2 _center, float _x, float  _y, float _degree)
    {
        float longAxis = _x > _y ? _x : _y;
        float shortAxis = _x > _y ? _y : _x;

        float csqr = longAxis * longAxis - shortAxis * shortAxis;
        float r = 2 * longAxis * longAxis / Mathf.Sqrt(csqr - 4 * csqr * Mathf.Cos(_degree * Mathf.Deg2Rad) * Mathf.Cos(_degree * Mathf.Deg2Rad) + 4 * longAxis * longAxis);
        float x = r * Mathf.Cos(_degree * Mathf.Deg2Rad);
        float y = r * Mathf.Sin(_degree * Mathf.Deg2Rad);

        return new Vector2(_center.x + x, _center.y + y);
    }

    public static Vector2 CircleCoord(Vector2 _center, float _radius, float _degree)
    {
        float x = _radius * Mathf.Cos(_degree * Mathf.Deg2Rad);
        float y = _radius * Mathf.Sin(_degree * Mathf.Deg2Rad);

        return new Vector2(_center.x + x, _center.y + y);
    }

    /// <summary>
    /// 클로버 모양 만드는 메소드
    /// </summary>
    /// <param name="_center"></param> 중심좌표
    /// <param name="_depth"></param> 클로버의 깊이 들어가는 정도
    /// <param name="_maxLength"></param> 클로버에서 중심으로부터 최대 길이
    /// <param name="_degree"></param> 각도
    /// <param name="_vertexCount"></param> 클로버의 꼭지점(중심으로부터 가장 긴 점 개수)
    /// <returns></returns>
    public static Vector2 Clover(Vector2 _center, float _depth, float _maxLength, float _degree, float _vertexCount)
    {
        float r = _maxLength - _maxLength * _depth * Mathf.Sin(_vertexCount * _degree);
        float x = r * Mathf.Cos(_degree);
        float y = r * Mathf.Sin(_degree);

        return new Vector2(_center.x + x, _center.y + y);
    }
}
