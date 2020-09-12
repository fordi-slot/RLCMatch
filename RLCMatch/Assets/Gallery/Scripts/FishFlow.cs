// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class FishFlow : MonoBehaviour
{

	Vector3 startPosition = Vector3.zero;
	Vector3 tmpMoveTan = Vector3.zero;
	float tmpValue = 0.1f;
	Vector3 tmpTran = Vector3.zero;
	float moveSpeed = 0.7f;
	float dists;
	Animation ani;
	CharacterController controller;
	void Start()
	{
		startPosition = transform.position;
		ani = GetComponent<Animation>();
		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		//计算游动距离
		float dist = Vector3.Distance(startPosition, transform.position);
		ani.Play();
		dists = Vector3.Distance(tmpTran, transform.position);
		tmpTran = transform.position;
		if (dists < 0.01f)
		{
			tmpMoveTan = Vector3.forward;
			tmpMoveTan = transform.TransformDirection(tmpMoveTan);
			tmpMoveTan *= 1;
			controller.Move(tmpMoveTan * Time.deltaTime);
			if (tmpValue > 0.5f)
			{
				transform.Rotate(0, 60, 0);
			}
			else
			{
				transform.Rotate(0, -60, 0);
			}
			return;
		}
		if (dist > 4)
		{//游动距离超过2米开始停歇
			tmpMoveTan = Vector3.forward;
			tmpMoveTan = transform.TransformDirection(tmpMoveTan);
			tmpMoveTan *= moveSpeed;
			controller.Move(tmpMoveTan * Time.deltaTime);
			if (dist > 2.5f)
			{//超过2.5f米开始掉头
				if (tmpValue > 0.5f)
				{
					transform.Rotate(Vector3.up * Time.deltaTime * 60);
				}
				else
				{
					transform.Rotate(-Vector3.up * Time.deltaTime * 60);
				}
			}
			if (dist > 2.8f)
			{
				startPosition = transform.position;
				//生成随机数
				tmpValue = Random.value;
			}
		}
		else
		{
			tmpMoveTan = Vector3.forward;
			tmpMoveTan = transform.TransformDirection(tmpMoveTan);
			tmpMoveTan *= moveSpeed + 0.3f;
			controller.Move(tmpMoveTan * Time.deltaTime);
		}
	}
	void Test() { }
}