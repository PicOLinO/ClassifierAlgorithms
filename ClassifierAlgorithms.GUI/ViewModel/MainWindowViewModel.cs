﻿#region Usings

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using ClassifierAlgorithms.Core;
using ClassifierAlgorithms.Core.Domain;
using DevExpress.Mvvm;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

#endregion

namespace ClassifierAlgorithms.GUI.ViewModel
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly Random random;

        public AsyncCommand GeneratePointsCommand { get; }
        public AsyncCommand ClassifyRandomPointCommand { get; }

        public MainWindowViewModel()
        {
            PlotModel = InitializePlot(0, 1);
            random = new Random();

            FirstClassExpectationX = 0.3;
            FirstClassExpectationY = 0.3;

            SecondClassExpectationX = 0.7;
            SecondClassExpectationY = 0.7;

            CorrelationMatrixInput = "0,005 0\n0 0,005";

            GeneratePointsCommand = new AsyncCommand(OnGeneratePoints);
            ClassifyRandomPointCommand = new AsyncCommand(OnClassifyRandomPoints, CanClassify);
        }

        private Class FirstClass { get; set; }
        private Class SecondClass { get; set; }

        private bool CanClassify()
        {
            return FirstClass != null && SecondClass != null;
        }

        private async Task OnClassifyRandomPoints()
        {
            await Task.Run(() =>
                           {
                               try
                               {
                                   var bayes = new BayesClassifier(FirstClass, SecondClass);
                                   var stopwatch = new Stopwatch();

                                   stopwatch.Start();
                                   for (var i = 0; i < 5000; i++)
                                   {
                                       var randomPointX = random.NextDouble() * (PlotModel.Axes[0].Maximum - PlotModel.Axes[0].Minimum) + PlotModel.Axes[0].Minimum;
                                       var randomPointY = random.NextDouble() * (PlotModel.Axes[1].Maximum - PlotModel.Axes[1].Minimum) + PlotModel.Axes[1].Minimum;

                                       var result = bayes.ClassifyByCorrelation(randomPointX, randomPointY, new Matrix(CorrelationMatrix));

                                       if (result == FirstClass)
                                       {
                                           FirstClassScatterSeries.Points.Add(new ScatterPoint(randomPointX, randomPointY, 4, double.NaN, FirstClass.Id));
                                       }
                                       else
                                       {
                                           SecondClassScatterSeries.Points.Add(new ScatterPoint(randomPointX, randomPointY, 4, double.NaN, FirstClass.Id));
                                       }

                                       if (stopwatch.Elapsed > TimeSpan.FromMilliseconds(100))
                                       {
                                           PlotModel.InvalidatePlot(true);
                                           stopwatch.Restart();
                                       }
                                   }

                                   PlotModel.InvalidatePlot(true);
                               }
                               catch (Exception e)
                               {
                                   MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                               }
                           });
        }


        private async Task OnGeneratePoints()
        {
            try
            {
            await Task.Run(() =>
                           {
                               const int countOfPoints = 500;

                               if (FirstClassScatterSeries.Points.Count != 0)
                               {
                                   FirstClassScatterSeries.Points.Clear();
                               }

                               if (SecondClassScatterSeries.Points.Count != 0)
                               {
                                   SecondClassScatterSeries.Points.Clear();
                               }

                               var generator = new Generator();

                               var firstClassGenerateTask = Task.Run(() =>
                                                                     {
                                                                         FirstClass = generator.GenerateClassByGaussian(countOfPoints, FirstClassExpectationX, FirstClassExpectationY, CorrelationMatrix);
                                                                         for (var i = 0; i < countOfPoints; i++)
                                                                         {
                                                                             var newPoint = new ScatterPoint(FirstClass.Vector[i, 0],
                                                                                                             FirstClass.Vector[i, 1]);
                                                                             FirstClassScatterSeries.Points.Add(newPoint);
                                                                         }
                                                                     });

                               var secondClassGenerateTask = Task.Run(() =>
                                                                      {
                                                                          SecondClass = generator.GenerateClassByGaussian(countOfPoints, SecondClassExpectationX, SecondClassExpectationY, CorrelationMatrix);
                                                                          for (var i = 0; i < countOfPoints; i++)
                                                                          {
                                                                              var newPoint = new ScatterPoint(SecondClass.Vector[i, 0],
                                                                                                              SecondClass.Vector[i, 1]);
                                                                              SecondClassScatterSeries.Points.Add(newPoint);
                                                                          }
                                                                      });

                               Task.WaitAll(firstClassGenerateTask, secondClassGenerateTask);
                               PlotModel.InvalidatePlot(true);
                           });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.InnerException?.Message ?? e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Plot

        private const string FirstClassScatterSeriesTag = "FirstClassScatterSeries";
        private const string SecondClassScatterSeriesTag = "SecondClassScatterSeries";
        private const string LineSeriesTag = "LineSeries";
        private ScatterSeries FirstClassScatterSeries { get; set; }
        private ScatterSeries SecondClassScatterSeries { get; set; }
        private LineSeries LineSeries { get; set; }

        private PlotModel InitializePlot(int min, int max)
        {
            var plotModel = new PlotModel();

            plotModel.Axes.Add(new LinearAxis {Position = AxisPosition.Left, Maximum = max, Minimum = min});
            plotModel.Axes.Add(new LinearAxis {Position = AxisPosition.Bottom, Maximum = max, Minimum = min});

            FirstClassScatterSeries = new ScatterSeries
                                      {
                                          Tag = FirstClassScatterSeriesTag,
                                          MarkerType = MarkerType.Cross,
                                          MarkerSize = 2,
                                          MarkerFill = OxyColors.Transparent,
                                          MarkerStrokeThickness = 1,
                                          MarkerStroke = OxyColors.DarkBlue
                                      };
            SecondClassScatterSeries = new ScatterSeries
                                       {
                                           Tag = SecondClassScatterSeriesTag,
                                           MarkerType = MarkerType.Cross,
                                           MarkerSize = 2,
                                           MarkerFill = OxyColors.Transparent,
                                           MarkerStrokeThickness = 1,
                                           MarkerStroke = OxyColors.DarkOrange
                                       };
            LineSeries = new LineSeries
                         {
                             Tag = LineSeriesTag,
                             MarkerType = MarkerType.None
                         };

            plotModel.Series.Add(FirstClassScatterSeries);
            plotModel.Series.Add(SecondClassScatterSeries);
            plotModel.Series.Add(LineSeries);

            return plotModel;
        }

        #endregion
    }
}