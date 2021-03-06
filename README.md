# Equalizer
Unity based equalizer project.

This project is an example that uses Unity's audio data and spectrum interfaces to analyze and filter audio. There are two main objects that can be modified to play with the project:

1. The AudioSource object, includes:
  - AudioSource component (where the audio source can be changed)
  - AudioFilterFactory component is required for any of the filters to work - this component implements the OnAudioFilterRead() such that all the filters are being processed together
  - AudioFilterPeakingFilter component is a required filter for the equalizer to work
  - AudioSynth component can be used to override the audio source with a simple synthesizer - be careful when playing with high frequencies and high gain, it can damage your speakers
  ![Image of AudioSource](https://github.com/shalevy2888/Equalizer/blob/master/Assets/Readme%20Resources/Audio%20Source%20and%20Filters.png)

2. The Equalizer object, includes:
  - AudioAnalyzer component is used to analyze the audio and display the frequency bands
  - Equalizer component is used to visualize the Peaking Filter
  ![Image of AudioSource](https://github.com/shalevy2888/Equalizer/blob/master/Assets/Readme%20Resources/Equalizer.png)

Credit: 
- The BiQuadFilter is from Mark Heath: https://github.com/naudio/NAudio
- Baseline code for analyzer taken from this thread: https://answers.unity.com/questions/157940/getoutputdata-and-getspectrumdata-they-represent-t.html
- Some ideas taken from Peer Play youtube channel: https://www.youtube.com/channel/UCBkub2TsbCFIfdhuxRr2Lrw
