﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace CurtinUniversity.MolecularDynamics.Model {

    public enum InteractionType {

        Attractive,
        Stable,
        Repulsive
    }

    public class AtomInteraction {

        public Atom Atom1;
        public Atom Atom2;
        public float Distance;
        public double SumOfAtomicRadii;
        public double? LennardJonesPotential;
        public double? ElectrostaticForce;
        public InteractionType? InteractionType;
        public Color? InteractionColour;

        public override int GetHashCode() {

            if (Atom1.Index < Atom2.Index) {
                return (Atom1.Index + "_" + Atom2.Index).GetHashCode();
            }
            else {
                return (Atom2.Index + "_" + Atom1.Index).GetHashCode();
            }
        }

        public override string ToString() {

            return
                "Atom1: " + Atom1 + "\n" +
                "Atom2: " + Atom2 + "\n" +
                "Distance: " + Distance + "\n" +
                "SumOfAtomicRadii: " + SumOfAtomicRadii + "\n" +
                "LJPotential: " + LennardJonesPotential + "\n" +
                "ElectrostaticForce: " + ElectrostaticForce + "\n" +
                "Colour: " + InteractionColour;
        }
    }

    public class InteractionsCalculator {

        private const float maxInteractionDistance = 0.8f;
        private const int maxInteractionsPerAtom = 5;

        private HashSet<int> addedInteractions;

        public List<AtomInteraction> GetAllInteractions(List<Atom> molecule1Atoms, List<Vector3> molecule1AtomPositions, List<Atom> molecule2Atoms, List<Vector3> molecule2AtomPositions, Gradient repulsiveGradient, Gradient strongAttractiveGradient, Gradient weakAttractiveGradient, int processorCores = 1) {

            if (molecule1Atoms == null || molecule1AtomPositions == null || molecule1Atoms.Count != molecule1AtomPositions.Count) {
                Debug.Log("Interactions calculator, Molecule 1 atoms count and molecule 1 atom positions count don't match");
                return new List<AtomInteraction>();
            }

            if (molecule2Atoms == null || molecule2AtomPositions == null || molecule2Atoms.Count != molecule2AtomPositions.Count) {
                Debug.Log("Interactions calculator, Molecule 2 atoms count and molecule 2 atom positions count don't match");
                return new List<AtomInteraction>();
            }

            KdTree<float, int> molecule2AtomTree = new KdTree<float, int>(3, new FloatMath());

            for (int i = 0; i < molecule2Atoms.Count; i++) {

                Vector3 molecule2AtomPosition = molecule2AtomPositions[i];
                molecule2AtomTree.Add(new float[] { molecule2AtomPosition.x, molecule2AtomPosition.y, molecule2AtomPosition.z }, i);
            }

            List<AtomInteraction> interactions = new List<AtomInteraction>();

            for (int i = 0; i < molecule1Atoms.Count; i++) {

                Atom atom = molecule1Atoms[i];
                Vector3 atomPosition = molecule1AtomPositions[i];

                KdTreeNode<float, int>[] interactingAtoms = molecule2AtomTree.RadialSearch(new float[] { atomPosition.x, atomPosition.y, atomPosition.z }, maxInteractionDistance, maxInteractionsPerAtom);

                foreach(KdTreeNode<float, int> node in interactingAtoms) {

                    Atom interactingAtom = molecule2Atoms[node.Value];

                    Vector3 interactingAtomPosition = molecule2AtomPositions[node.Value];

                    // get distance between atoms
                    float deltaX = interactingAtomPosition.x - atomPosition.x;
                    float deltaY = interactingAtomPosition.y - atomPosition.y;
                    float deltaZ = interactingAtomPosition.z - atomPosition.z;
                    float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

                    AtomInteraction interaction = new AtomInteraction() {
                        Atom1 = atom,
                        Atom2 = interactingAtom,
                        Distance = distance,
                    };

                    SetLennardJonesPotential(interaction);
                    SetElecrostaticForce(interaction);
                    SetInteractionType(interaction, repulsiveGradient, strongAttractiveGradient, weakAttractiveGradient);

                    interactions.Add(interaction);
                }
            }

            return interactions;
        }

        public List<AtomInteraction> GetClosestInteractions(List<AtomInteraction> interactions) {

            List<AtomInteraction> sortedInteractions = interactions.OrderBy(o => o.Distance).ToList();
            List<AtomInteraction> closestInteractions = new List<AtomInteraction>();

            HashSet<int> usedAtoms = new HashSet<int>();

            foreach(AtomInteraction interaction in sortedInteractions) {

                if(!usedAtoms.Contains(interaction.Atom1.Index) && !usedAtoms.Contains(interaction.Atom2.Index)) {

                    closestInteractions.Add(interaction);
                    usedAtoms.Add(interaction.Atom1.Index);
                    usedAtoms.Add(interaction.Atom2.Index);
                }
            }

            return closestInteractions;
        }

        // Calculates the Lennard-Jones Potential for two atoms
        public void SetLennardJonesPotential(AtomInteraction interaction) {

            if (interaction.Distance >= 8d) {
                return;
            }

            AtomSigmaEpsilon atom1SigmaEpsilon = InteractionForces.GetAtomSigmaEpsilon(interaction.Atom1);
            AtomSigmaEpsilon atom2SigmaEpsilon = InteractionForces.GetAtomSigmaEpsilon(interaction.Atom2);

            interaction.SumOfAtomicRadii = ((double)atom1SigmaEpsilon.Sigma + (double)atom2SigmaEpsilon.Sigma) / 2d;

            double energyWellDepth = Math.Sqrt((double)atom1SigmaEpsilon.Epsilon * (double)atom2SigmaEpsilon.Epsilon);
            double distanceBetweenAtoms6 = Math.Pow(interaction.Distance, 6);
            double distanceBetweenAtoms12 = Math.Pow(interaction.Distance, 12);
            double sumOfAtomicRadii6 = Math.Pow(interaction.SumOfAtomicRadii, 6);
            double sumOfAtomicRadii12 = Math.Pow(interaction.SumOfAtomicRadii, 12);

            double score = energyWellDepth * ((sumOfAtomicRadii12 / distanceBetweenAtoms12) - (2d * sumOfAtomicRadii6 / distanceBetweenAtoms6));
            interaction.LennardJonesPotential = Double.IsPositiveInfinity(score) ? Double.MaxValue : score;
        }

        public void SetElecrostaticForce(AtomInteraction interaction) {

            double distanceInMetres = interaction.Distance * 0.000000001d; // 1e-9;

            //double coulombsConstant = 8987551787;
            double coulombsConstant = 332;

            double chargeAtom1 = 0;
            double chargeAtom2 = 0;

            double electrostaticForce = 332d * (chargeAtom1 * chargeAtom2) / (distanceInMetres * distanceInMetres);

            interaction.ElectrostaticForce = 0;
        }

        // Sets a colour calculated on distance compared to atom sigmas
        public void SetInteractionType(AtomInteraction interaction, Gradient repulsiveGradient, Gradient strongAttractiveGradient, Gradient weakAttractiveGradient) {

            float weakAttractiveMaxDistance = 2.5f * (float)interaction.SumOfAtomicRadii;

            if (interaction.Distance >= weakAttractiveMaxDistance) {

                interaction.InteractionType = null;
                return;
            }

            float strongAttractiveMaxDistance = 1.5f * (float)interaction.SumOfAtomicRadii;
            float repulsiveMaxDistance = 0.9f * (float)interaction.SumOfAtomicRadii;

            if (interaction.Distance >= strongAttractiveMaxDistance) {

                interaction.InteractionType = InteractionType.Attractive;
                float weakForce = (interaction.Distance - strongAttractiveMaxDistance) / (weakAttractiveMaxDistance - strongAttractiveMaxDistance);
                interaction.InteractionColour = weakAttractiveGradient.Evaluate(weakForce);
            }
            else if (interaction.Distance >= repulsiveMaxDistance) {

                interaction.InteractionType = InteractionType.Stable;
                float strongForce = (interaction.Distance - repulsiveMaxDistance) / (strongAttractiveMaxDistance - repulsiveMaxDistance);
                interaction.InteractionColour = strongAttractiveGradient.Evaluate(strongForce);
            }
            else {

                interaction.InteractionType = InteractionType.Repulsive;
                float repulsiveForce = interaction.Distance / repulsiveMaxDistance;
                interaction.InteractionColour = repulsiveGradient.Evaluate(repulsiveForce);
            }
        }
    }
}
