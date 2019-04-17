﻿using System;
using ClassifierAlgorithms.Core.Domain;
using ClassifierAlgorithms.Core.Services;

namespace ClassifierAlgorithms.Core
{
    public class BayesClassifier
    {
        private readonly Class firstClass;
        private readonly Class secondClass;
        private readonly double[,] covariationMatrix;
        private readonly MatrixService matrixService;

        public BayesClassifier(Class firstClass, Class secondClass, double[,] covariationMatrix)
        {
            this.firstClass = firstClass;
            this.secondClass = secondClass;
            this.covariationMatrix = covariationMatrix;

            matrixService = new MatrixService();
        }

        public void Calculate(double x, double y)
        {
            var probabilityX = BayesFunction(x, firstClass);
            var probabilityY = BayesFunction(y, secondClass);
        }

        private double BayesFunction(double value, Class data)
        {
            return (1 / (Math.Pow(2 * Math.PI, 1 / 2d) * (Math.Pow(matrixService.GetDeterminant(covariationMatrix), 1 / 2d))))
                    * Math.Exp(-0.5 * ((value - data.Expectation) / (/*Ei(x-ui)*/)));
        }

        //TODO:
    }
}