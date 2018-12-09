# Equalizer
Unity based equalizer project.

This project is an example demo that uses Unity's audio data and spectrum interfaces to analyze and filter the audio. There are two main objects that can be modified to play with the project:

1. The AudioSource object, includes
  - AudioSource component (where the audio source can be changed)
  - AudioFilterFactory component is required for any of the filters to work
  - AudioFilterPeakingFilter component is required for the equalier to work
  - AudioSynth component can be used to override the audio source with a simple synthesizer - be careful when playing with high frequencies and high gain, it can damage your speakers
  ![Image of AudioSource](https://github.com/shalevy2888/Equalizer/blob/master/Assets/Readme%20Resources/Audio%20Source%20and%20Filters.png)

2. The Equalizer object, includes:
  - AudioAnalyzer component is used to analyze the audio and display the frequency bands
  - Equalizer component is used to visualize the Peaking Filter
  ![Image of AudioSource](https://github.com/shalevy2888/Equalizer/blob/master/Assets/Readme%20Resources/Equalizer.png)

