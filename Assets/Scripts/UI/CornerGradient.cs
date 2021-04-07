// Adapted from: https://github.com/azixMcAze/Unity-UIGradient

using UnityEngine;
using UnityEngine.UI;

public class CornerGradient : BaseMeshEffect
{
	public Color m_topLeftColor = Color.white;
	public Color m_topRightColor = Color.white;
	public Color m_bottomRightColor = Color.white;
	public Color m_bottomLeftColor = Color.white;

	public override void ModifyMesh(VertexHelper vh)
	{
		if (enabled)
		{
			Rect rect = graphic.rectTransform.rect;
			Matrix2x3 localPositionMatrix = LocalPositionMatrix(rect, Vector2.right);

			UIVertex vertex = default;
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref vertex, i);
				Vector2 normalizedPosition = localPositionMatrix * vertex.position;
				vertex.color *= Bilerp(m_bottomLeftColor, m_bottomRightColor, m_topLeftColor, m_topRightColor, normalizedPosition);
				vh.SetUIVertex(vertex, i);
			}
		}
	}

	struct Matrix2x3
	{
		public float m00, m01, m02, m10, m11, m12;
		public Matrix2x3(float m00, float m01, float m02, float m10, float m11, float m12)
		{
			this.m00 = m00;
			this.m01 = m01;
			this.m02 = m02;
			this.m10 = m10;
			this.m11 = m11;
			this.m12 = m12;
		}

		public static Vector2 operator *(Matrix2x3 m, Vector2 v)
		{
			float x = (m.m00 * v.x) - (m.m01 * v.y) + m.m02;
			float y = (m.m10 * v.x) + (m.m11 * v.y) + m.m12;
			return new Vector2(x, y);
		}
	}

	static Matrix2x3 LocalPositionMatrix(Rect rect, Vector2 dir)
	{
		float cos = dir.x;
		float sin = dir.y;
		Vector2 rectMin = rect.min;
		Vector2 rectSize = rect.size;
		const float c = 0.5f;
		float ax = (rectMin.x / rectSize.x) + c;
		float ay = (rectMin.y / rectSize.y) + c;
		float m00 = cos / rectSize.x;
		float m01 = sin / rectSize.y;
		float m02 = -((ax * cos) - (ay * sin) - c);
		float m10 = sin / rectSize.x;
		float m11 = cos / rectSize.y;
		float m12 = -((ax * sin) + (ay * cos) - c);
		return new Matrix2x3(m00, m01, m02, m10, m11, m12);
	}

	static Color Bilerp(Color a1, Color a2, Color b1, Color b2, Vector2 t)
	{
		Color a = Color.LerpUnclamped(a1, a2, t.x);
		Color b = Color.LerpUnclamped(b1, b2, t.x);
		return Color.LerpUnclamped(a, b, t.y);
	}
}
