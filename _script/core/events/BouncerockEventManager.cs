

using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

namespace Bouncerock.Events
{
	public struct BouncerockGameEvent
	{
		public string EventName;
		public BouncerockGameEvent(string newName)
		{
			EventName = newName;
		}
	}
	public static class BouncerockEventManager
	{
		private static Dictionary<Type, List<BouncerockEventListenerBase>> _subbersList;

		static BouncerockEventManager()
		{
			_subbersList = new Dictionary<Type, List<BouncerockEventListenerBase>>();
		}

	
		public static void AddListener<BouncerockEvent>(BouncerockEventListener<BouncerockEvent> listener) where BouncerockEvent : struct
		{
			Type eventType = typeof(BouncerockEvent);

			if (!_subbersList.ContainsKey(eventType))
				_subbersList[eventType] = new List<BouncerockEventListenerBase>();

			if (!SubscriptionExists(eventType, listener))
				_subbersList[eventType].Add(listener);
		}

		public static void RemoveListener<BouncerockEvent>(BouncerockEventListener<BouncerockEvent> listener) where BouncerockEvent : struct
		{
			Type eventType = typeof(BouncerockEvent);

			if (!_subbersList.ContainsKey(eventType))
			{
			}

			List<BouncerockEventListenerBase> subscriberList = _subbersList[eventType];
			bool listenerFound;
			listenerFound = false;

			if (listenerFound)
			{

			}

			for (int i = 0; i < subscriberList.Count; i++)
			{
				if (subscriberList[i] == listener)
				{
					subscriberList.Remove(subscriberList[i]);
					listenerFound = true;

					if (subscriberList.Count == 0)
						_subbersList.Remove(eventType);

					return;
				}
			}
		}

		public static void TriggerEvent<BouncerockEvent>(BouncerockEvent newEvent) where BouncerockEvent : struct
		{
			List<BouncerockEventListenerBase> list;
			if (!_subbersList.TryGetValue(typeof(BouncerockEvent), out list))

				return;

			for (int i = 0; i < list.Count; i++)
			{
				(list[i] as BouncerockEventListener<BouncerockEvent>).OnBouncerockEvent(newEvent);
			}
		}
		
		private static bool SubscriptionExists(Type type, BouncerockEventListenerBase receiver)
		{
			List<BouncerockEventListenerBase> receivers;

			if (!_subbersList.TryGetValue(type, out receivers)) return false;

			bool exists = false;

			for (int i = 0; i < receivers.Count; i++)
			{
				if (receivers[i] == receiver)
				{
					exists = true;
					break;
				}
			}

			return exists;
		}
	}



	public static class EventRegister
	{
		public delegate void Delegate<T>(T eventType);

		public static void BouncerockEventStartListening<EventType>(this BouncerockEventListener<EventType> caller) where EventType : struct
		{
			BouncerockEventManager.AddListener<EventType>(caller);
		}

		public static void BouncerockEventStopListening<EventType>(this BouncerockEventListener<EventType> caller) where EventType : struct
		{
			BouncerockEventManager.RemoveListener<EventType>(caller);
		}
	}

	
	
	public interface BouncerockEventListenerBase { };

	
	
	public interface BouncerockEventListener<T> : BouncerockEventListenerBase
	{
		void OnBouncerockEvent(T eventType);
	}
}