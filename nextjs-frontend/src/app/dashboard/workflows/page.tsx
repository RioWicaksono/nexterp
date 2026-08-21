'use client';

import { useState } from 'react';
import { PageHeader } from '@/components/PageHeader';
import { Plus, Trash2, Settings, GripVertical, Check, X, ChevronRight, AlertCircle, Save } from 'lucide-react';
import { cn } from '@/lib/utils';

interface WorkflowStep {
  id: string;
  name: string;
  approvers: string[];
  type: 'approval' | 'condition' | 'notification';
  threshold?: number;
  deadline?: number;
}

interface Workflow {
  id: string;
  name: string;
  entityType: string;
  steps: WorkflowStep[];
  isActive: boolean;
}

const mockWorkflows: Workflow[] = [
  {
    id: '1',
    name: 'Purchase Order Approval',
    entityType: 'purchase_order',
    isActive: true,
    steps: [
      { id: 's1', name: 'Manager Review', approvers: ['Manager'], type: 'approval', threshold: 2, deadline: 3 },
      { id: 's2', name: 'Finance Review', approvers: ['Finance Head', 'CFO'], type: 'approval', threshold: 1, deadline: 5 },
    ],
  },
  {
    id: '2',
    name: 'Expense Report Approval',
    entityType: 'expense',
    isActive: true,
    steps: [
      { id: 's1', name: 'Supervisor Approval', approvers: ['Supervisor'], type: 'approval', threshold: 1, deadline: 2 },
    ],
  },
];

export default function WorkflowsPage() {
  const [workflows] = useState<Workflow[]>(mockWorkflows);
  const [selectedWorkflow, setSelectedWorkflow] = useState<Workflow | null>(mockWorkflows[0] || null);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Approval Workflows"
        subtitle="Design and manage approval processes"
        breadcrumbs={[
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Workflows' },
        ]}
        actions={
          <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition">
            <Plus className="w-4 h-4" />
            Create Workflow
          </button>
        }
      />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Workflow List */}
        <div className="lg:col-span-1 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
          <div className="p-4 border-b border-slate-200 dark:border-slate-700">
            <h3 className="font-semibold text-slate-900 dark:text-white">Workflows</h3>
          </div>
          <div className="divide-y divide-slate-100 dark:divide-slate-700">
            {workflows.map((workflow) => (
              <button
                key={workflow.id}
                onClick={() => setSelectedWorkflow(workflow)}
                className={cn(
                  'w-full p-4 text-left transition',
                  selectedWorkflow?.id === workflow.id
                    ? 'bg-blue-50 dark:bg-blue-900/20'
                    : 'hover:bg-slate-50 dark:hover:bg-slate-700/30'
                )}
              >
                <div className="flex items-center justify-between">
                  <span className="font-medium text-slate-900 dark:text-white">{workflow.name}</span>
                  {workflow.isActive ? (
                    <span className="text-xs px-2 py-0.5 bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400 rounded-full">Active</span>
                  ) : (
                    <span className="text-xs px-2 py-0.5 bg-slate-100 dark:bg-slate-700 text-slate-500 rounded-full">Inactive</span>
                  )}
                </div>
                <p className="text-sm text-slate-500 mt-1">
                  {workflow.steps.length} step{workflow.steps.length !== 1 ? 's' : ''}
                </p>
              </button>
            ))}
          </div>
        </div>

        {/* Workflow Builder */}
        <div className="lg:col-span-2 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
          {selectedWorkflow ? (
            <WorkflowBuilder workflow={selectedWorkflow} />
          ) : (
            <div className="flex flex-col items-center justify-center py-16 text-slate-400">
              <Settings className="w-12 h-12 mb-4 opacity-50" />
              <p>Select a workflow to edit</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function WorkflowBuilder({ workflow }: { workflow: Workflow }) {
  const [steps, setSteps] = useState<WorkflowStep[]>(workflow.steps);
  const [isEditing, setIsEditing] = useState(false);

  const addStep = () => {
    const newStep: WorkflowStep = {
      id: `step_${Date.now()}`,
      name: 'New Step',
      approvers: ['Approver'],
      type: 'approval',
      threshold: 1,
      deadline: 3,
    };
    setSteps([...steps, newStep]);
    setIsEditing(true);
  };

  const removeStep = (stepId: string) => {
    setSteps(steps.filter((s) => s.id !== stepId));
  };

  const updateStep = (stepId: string, updates: Partial<WorkflowStep>) => {
    setSteps(steps.map((s) => (s.id === stepId ? { ...s, ...updates } : s)));
  };

  return (
    <>
      <div className="p-4 border-b border-slate-200 dark:border-slate-700 flex items-center justify-between">
        <div>
          <h3 className="font-semibold text-slate-900 dark:text-white">{workflow.name}</h3>
          <p className="text-sm text-slate-500">Visual workflow builder</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={addStep}
            className="flex items-center gap-1 px-3 py-1.5 text-sm border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 transition"
          >
            <Plus className="w-4 h-4" />
            Add Step
          </button>
          <button
            onClick={() => setIsEditing(!isEditing)}
            className="flex items-center gap-1 px-3 py-1.5 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition"
          >
            <Save className="w-4 h-4" />
            {isEditing ? 'Save Changes' : 'Edit'}
          </button>
        </div>
      </div>

      <div className="p-6">
        {/* Visual Workflow */}
        <div className="space-y-4">
          {steps.map((step, index) => (
            <div key={step.id} className="relative">
              {/* Connector Line */}
              {index > 0 && (
                <div className="absolute -top-6 left-6 w-0.5 h-6 bg-slate-300 dark:bg-slate-600" />
              )}

              <div className="flex items-start gap-4">
                {/* Step Number */}
                <div className="w-12 h-12 rounded-full bg-blue-600 text-white flex items-center justify-center font-bold text-lg shrink-0">
                  {index + 1}
                </div>

                {/* Step Card */}
                <div className="flex-1 bg-slate-50 dark:bg-slate-900 rounded-lg p-4">
                  <div className="flex items-center justify-between mb-3">
                    <input
                      type="text"
                      value={step.name}
                      onChange={(e) => updateStep(step.id, { name: e.target.value })}
                      disabled={!isEditing}
                      className="font-semibold text-slate-900 dark:text-white bg-transparent border-0 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded px-1 py-0.5 -ml-1"
                    />
                    <div className="flex items-center gap-2">
                      {step.deadline && (
                        <span className="text-xs px-2 py-1 bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400 rounded-full">
                          {step.deadline} days
                        </span>
                      )}
                      {isEditing && (
                        <button
                          onClick={() => removeStep(step.id)}
                          className="p-1 text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded transition"
                          title="Remove step"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </div>

                  {/* Step Details */}
                  <div className="space-y-3">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="text-sm text-slate-500">Approvers:</span>
                      <div className="flex flex-wrap gap-1">
                        {step.approvers.map((approver, i) => (
                          <span
                            key={i}
                            className="text-sm px-2 py-1 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded"
                          >
                            {approver}
                          </span>
                        ))}
                      </div>
                    </div>

                    {step.threshold && (
                      <div className="flex items-center gap-2">
                        <span className="text-sm text-slate-500">Threshold:</span>
                        <span className="text-sm font-medium text-slate-700 dark:text-slate-300">
                          {step.threshold} of {step.approvers.length} approvers
                        </span>
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {/* Arrow */}
              <div className="absolute left-6 -bottom-4">
                <ChevronRight className="w-6 h-6 text-slate-400" />
              </div>
            </div>
          ))}

          {/* End Node */}
          {steps.length > 0 && (
            <div className="relative pt-8">
              <div className="flex items-center gap-4">
                <div className="w-12 h-12 rounded-full bg-green-600 text-white flex items-center justify-center shrink-0">
                  <Check className="w-6 h-6" />
                </div>
                <div className="flex-1 bg-green-50 dark:bg-green-900/20 rounded-lg p-4">
                  <span className="font-semibold text-green-700 dark:text-green-400">Approved</span>
                </div>
              </div>
            </div>
          )}

          {steps.length === 0 && (
            <div className="flex flex-col items-center justify-center py-12 text-slate-400">
              <AlertCircle className="w-8 h-8 mb-2 opacity-50" />
              <p>No steps defined</p>
              <button
                onClick={addStep}
                className="mt-4 text-blue-600 hover:underline"
              >
                Add first step
              </button>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
