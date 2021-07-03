# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Definitions for testing selectors
#

Document 'Selector.WithInputObject' -With 'GeneratorSelector' {
    Metadata @{
        Name = $PSDocs.TargetObject.Name
    }
    $PSDocs.TargetObject.Name
}
