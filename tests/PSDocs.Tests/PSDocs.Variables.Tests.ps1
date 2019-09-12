#
# Unit tests for the PSDocs automatic variables
#

Describe 'PSDocs variables' -Tag 'Variables' {
    Context 'PowerShell automatic variables' {
        document 'PSAutomaticVariables' {
            "PWD=$PWD"
        }

        It '$PWD' {
            $result = (PSAutomaticVariables -PassThru).Replace("`r`n", "`n").Replace("`r", "`n").Split("`n");
            $result | Where-Object -FilterScript { $_ -like "PWD=*" } | Should -Be "PWD=$PWD";
        }
    }

    Context 'PSDocs automatic variables' {
        document 'PSDocsAutomaticVariables' {
            Title '001'
            Metadata @{
                author = '002'
            }
            "Document.Title=$($document.Title)"
            "Document.Metadata=$($document.Metadata['author'])"
        }

        It '$Document' {
            $result = (PSDocsAutomaticVariables -PassThru).Replace("`r`n", "`n").Replace("`r", "`n").Split("`n");
            $result | Where-Object -FilterScript { $_ -like "Document.Title=*" } | Should -Be "Document.Title=001";
            $result | Where-Object -FilterScript { $_ -like "Document.Metadata=*" } | Should -Be "Document.Metadata=002";
        }
    }
}
