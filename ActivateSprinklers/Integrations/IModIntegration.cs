namespace ActivateSprinklers.Integrations {

	internal interface IModIntegration {
		string Label { get; }

		bool IsLoaded { get; }
	}
}