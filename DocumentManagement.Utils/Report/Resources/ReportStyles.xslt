<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <w:document xmlns:wpc="http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas" xmlns:cx="http://schemas.microsoft.com/office/drawing/2014/chartex" xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:m="http://schemas.openxmlformats.org/officeDocument/2006/math" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:wp14="http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing" xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing" xmlns:w10="urn:schemas-microsoft-com:office:word" xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" xmlns:w14="http://schemas.microsoft.com/office/word/2010/wordml" xmlns:w15="http://schemas.microsoft.com/office/word/2012/wordml" xmlns:w16se="http://schemas.microsoft.com/office/word/2015/wordml/symex" xmlns:wpg="http://schemas.microsoft.com/office/word/2010/wordprocessingGroup" xmlns:wpi="http://schemas.microsoft.com/office/word/2010/wordprocessingInk" xmlns:wne="http://schemas.microsoft.com/office/word/2006/wordml" xmlns:wps="http://schemas.microsoft.com/office/word/2010/wordprocessingShape" mc:Ignorable="w14 w15 w16se wp14">
      <w:body>
        <xsl:for-each select="Report">
        <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="00AB36EE">
          <w:pPr>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
        </w:p>
        <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="0054352D">
          <w:pPr>
            <w:jc w:val="center"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
        </w:p>
        <w:p w:rsidR="00C5707F" w:rsidRPr="0054352D" w:rsidRDefault="00C5707F" w:rsidP="0054352D">
          <w:pPr>
            <w:jc w:val="center"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:b/>
              <w:sz w:val="36"/>
              <w:szCs w:val="36"/>
              <w:lang w:val="en-US"/>
            </w:rPr>
          </w:pPr>
          <w:r>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:b/>
              <w:sz w:val="36"/>
              <w:szCs w:val="36"/>
            </w:rPr>
            <w:t>ОТЧЕТНЫЙ ЛИСТ</w:t>
          </w:r>
          <w:r w:rsidRPr="00C5707F">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:b/>
              <w:sz w:val="36"/>
              <w:szCs w:val="36"/>
            </w:rPr>
            <w:t xml:space="preserve"> </w:t>
          </w:r>
          <w:r>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:b/>
              <w:sz w:val="36"/>
              <w:szCs w:val="36"/>
            </w:rPr>
            <w:t>№</w:t>
          </w:r>
          <w:r w:rsidR="0054352D">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:b/>
              <w:sz w:val="36"/>
              <w:szCs w:val="36"/>
              <w:lang w:val="en-US"/>
            </w:rPr>
            <!-- Project Number placeholder replaced by the Project's Number attribute in the XML data file. -->
            <w:t xml:space="preserve"> <xsl:value-of select="@number"/> </w:t>
          </w:r>
        </w:p>
        <w:p w:rsidR="001A022E" w:rsidRPr="00C47584" w:rsidRDefault="001A022E" w:rsidP="0054352D">
          <w:pPr>
            <w:jc w:val="center"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="24"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
          <w:r w:rsidRPr="00C47584">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="24"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:t xml:space="preserve">по </w:t>
          </w:r>
          <w:r w:rsidR="00C70E09" w:rsidRPr="00C47584">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="24"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:t>проекту</w:t>
          </w:r>
          <w:r w:rsidRPr="00C47584">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="24"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:t>: «</w:t>
          </w:r>
          <w:r w:rsidR="00C70E09" w:rsidRPr="00C47584">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:i/>
              <w:sz w:val="24"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <!-- Project Name placeholder replaced by the Project's Name attribute in the XML data file. -->
            <w:t>
              <xsl:value-of select="@project"/>
            </w:t>
          </w:r>
          <w:r w:rsidRPr="00C47584">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="24"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:t>»</w:t>
          </w:r>
        </w:p>
        <w:p w:rsidR="00C70E09" w:rsidRDefault="00C70E09" w:rsidP="0054352D">
          <w:pPr>
            <w:jc w:val="center"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
        </w:p>
        <w:p w:rsidR="00AB36EE" w:rsidRPr="00C47584" w:rsidRDefault="00AB36EE" w:rsidP="001A022E">
          <w:pPr>
            <w:jc w:val="center"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
        </w:p>
        <w:p w:rsidR="00C70E09" w:rsidRDefault="00C70E09" w:rsidP="001A022E">
          <w:pPr>
            <w:jc w:val="center"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
        </w:p>
        <w:p w:rsidR="0054352D" w:rsidRDefault="0054352D" w:rsidP="001A022E">
          <w:pPr>
            <w:jc w:val="center"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
        </w:p>
        <w:p w:rsidR="0054352D" w:rsidRPr="00C47584" w:rsidRDefault="0054352D" w:rsidP="001A022E">
          <w:pPr>
            <w:jc w:val="center"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
        </w:p>
        <w:p w:rsidR="009504FF" w:rsidRPr="00C47584" w:rsidRDefault="009504FF" w:rsidP="001A022E">
          <w:pPr>
            <w:jc w:val="center"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
        </w:p>
        <w:tbl>
          <w:tblPr>
            <w:tblW w:w="14776" w:type="dxa"/>
            <w:tblInd w:w="284" w:type="dxa"/>
            <w:tblLook w:val="04A0" w:firstRow="1" w:lastRow="0" w:firstColumn="1" w:lastColumn="0" w:noHBand="0" w:noVBand="1"/>
          </w:tblPr>
          <w:tblGrid>
            <w:gridCol w:w="3982"/>
            <w:gridCol w:w="5794"/>
            <w:gridCol w:w="2268"/>
            <w:gridCol w:w="2732"/>
          </w:tblGrid>
          <w:tr w:rsidR="00B32DE5" w:rsidRPr="00C47584" w:rsidTr="00AB36EE">
            <w:trPr>
              <w:trHeight w:val="574"/>
            </w:trPr>
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="3982" w:type="dxa"/>
                <w:shd w:val="clear" w:color="auto" w:fill="auto"/>
              </w:tcPr>
              <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="00C70E09">
                <w:pPr>
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:sz w:val="28"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                </w:pPr>
                <w:r w:rsidRPr="00C47584">
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:sz w:val="28"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                  <w:t>___________________________</w:t>
                </w:r>
              </w:p>
            </w:tc>
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="5794" w:type="dxa"/>
                <w:shd w:val="clear" w:color="auto" w:fill="auto"/>
              </w:tcPr>
              <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="00B32DE5">
                <w:pPr>
                  <w:jc w:val="right"/>
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:sz w:val="28"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                </w:pPr>
              </w:p>
            </w:tc>
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="2268" w:type="dxa"/>
                <w:shd w:val="clear" w:color="auto" w:fill="auto"/>
              </w:tcPr>
              <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="00B32DE5">
                <w:pPr>
                  <w:jc w:val="right"/>
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:sz w:val="28"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                </w:pPr>
                <w:r w:rsidRPr="00C47584">
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:sz w:val="28"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                  <w:t>____________//</w:t>
                </w:r>
              </w:p>
            </w:tc>
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="2732" w:type="dxa"/>
                <w:shd w:val="clear" w:color="auto" w:fill="auto"/>
              </w:tcPr>
              <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="00C70E09">
                <w:pPr>
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:sz w:val="28"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                </w:pPr>
                <w:r w:rsidRPr="00C47584">
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:sz w:val="28"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                  <w:t>_________________</w:t>
                </w:r>
              </w:p>
            </w:tc>
          </w:tr>
          <w:tr w:rsidR="00B32DE5" w:rsidRPr="00C47584" w:rsidTr="00AB36EE">
            <w:trPr>
              <w:trHeight w:val="459"/>
            </w:trPr>
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="3982" w:type="dxa"/>
                <w:shd w:val="clear" w:color="auto" w:fill="auto"/>
              </w:tcPr>
              <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="00B32DE5">
                <w:pPr>
                  <w:jc w:val="center"/>
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:i/>
                    <w:sz w:val="20"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                </w:pPr>
                <w:r w:rsidRPr="00C47584">
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:i/>
                    <w:sz w:val="20"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                  <w:t>Должность</w:t>
                </w:r>
              </w:p>
            </w:tc>
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="5794" w:type="dxa"/>
                <w:shd w:val="clear" w:color="auto" w:fill="auto"/>
              </w:tcPr>
              <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="00B32DE5">
                <w:pPr>
                  <w:jc w:val="center"/>
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:i/>
                    <w:sz w:val="20"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                </w:pPr>
              </w:p>
            </w:tc>
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="2268" w:type="dxa"/>
                <w:shd w:val="clear" w:color="auto" w:fill="auto"/>
              </w:tcPr>
              <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="00B32DE5">
                <w:pPr>
                  <w:jc w:val="center"/>
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:i/>
                    <w:sz w:val="20"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                </w:pPr>
                <w:r w:rsidRPr="00C47584">
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:i/>
                    <w:sz w:val="20"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                  <w:t>Подпись</w:t>
                </w:r>
              </w:p>
            </w:tc>
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="2732" w:type="dxa"/>
                <w:shd w:val="clear" w:color="auto" w:fill="auto"/>
              </w:tcPr>
              <w:p w:rsidR="00C70E09" w:rsidRPr="00C47584" w:rsidRDefault="00C70E09" w:rsidP="00B32DE5">
                <w:pPr>
                  <w:jc w:val="center"/>
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:i/>
                    <w:sz w:val="20"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                </w:pPr>
                <w:r w:rsidRPr="00C47584">
                  <w:rPr>
                    <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
                    <w:i/>
                    <w:sz w:val="20"/>
                    <w:szCs w:val="28"/>
                  </w:rPr>
                  <w:t>Расшифровка</w:t>
                </w:r>
              </w:p>
            </w:tc>
          </w:tr>
        </w:tbl>
        <w:p w:rsidR="00C5707F" w:rsidRDefault="0098404A" w:rsidP="0098404A">
          <w:pPr>
            <w:tabs>
              <w:tab w:val="center" w:pos="5244"/>
              <w:tab w:val="right" w:pos="10488"/>
            </w:tabs>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
          <w:r w:rsidRPr="00C47584">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:tab/>
          </w:r>
          <w:r w:rsidRPr="00C47584">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:tab/>
          </w:r>
          <w:r w:rsidR="00AB36EE">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:tab/>
          </w:r>
          <w:r w:rsidR="00AB36EE">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:tab/>
          </w:r>
          <w:r w:rsidR="00AB36EE">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:tab/>
          </w:r>
        </w:p>
        <w:p w:rsidR="00C5707F" w:rsidRDefault="00883877" w:rsidP="00C5707F">
          <w:pPr>
            <w:tabs>
              <w:tab w:val="center" w:pos="5244"/>
              <w:tab w:val="right" w:pos="10488"/>
            </w:tabs>
            <w:jc w:val="right"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
          <w:r w:rsidRPr="00C47584">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:t xml:space="preserve">от </w:t>
          </w:r>
          <w:r w:rsidR="00C70E09" w:rsidRPr="00C47584">
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <!-- Date placeholder replaced by the date attribute in the XML data file. -->
            <w:t>
              <xsl:value-of select="@date"/>
            </w:t>
          </w:r>
        </w:p>
        <w:p w:rsidR="00C5707F" w:rsidRDefault="00C5707F" w:rsidP="00C5707F">
          <w:pPr>
            <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
          <w:r>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:br w:type="page"/>
          </w:r>
        </w:p>
        <w:tbl>
          <w:tblPr>
            <w:tblStyle w:val="a7"/>
            <w:tblW w:w="15593" w:type="dxa"/>
            <w:tblInd w:w="-147" w:type="dxa"/>
            <w:tblLook w:val="04A0" w:firstRow="1" w:lastRow="0" w:firstColumn="1" w:lastColumn="0" w:noHBand="0" w:noVBand="1"/>
          </w:tblPr>
          <w:tblGrid>
            <w:gridCol w:w="9356"/>
            <w:gridCol w:w="6237"/>
          </w:tblGrid>
          <w:tr w:rsidR="00C5707F" w:rsidTr="00D5249A">
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="9356" w:type="dxa"/>
              </w:tcPr>
              <w:p w:rsidR="00C5707F" w:rsidRPr="00432EB6" w:rsidRDefault="00C5707F" w:rsidP="00D5249A">
                <w:pPr>
                  <w:spacing w:before="240" w:line="120" w:lineRule="auto"/>
                  <w:jc w:val="center"/>
                  <w:rPr>
                    <w:b/>
                    <w:bCs/>
                  </w:rPr>
                </w:pPr>
                <w:r w:rsidRPr="00432EB6">
                  <w:rPr>
                    <w:b/>
                    <w:bCs/>
                  </w:rPr>
                  <w:lastRenderedPageBreak/>
                  <w:t>Скриншот</w:t>
                </w:r>
              </w:p>
            </w:tc>
            <w:tc>
              <w:tcPr>
                <w:tcW w:w="6237" w:type="dxa"/>
              </w:tcPr>
              <w:p w:rsidR="00C5707F" w:rsidRPr="00432EB6" w:rsidRDefault="00C5707F" w:rsidP="00D5249A">
                <w:pPr>
                  <w:spacing w:before="240" w:line="120" w:lineRule="auto"/>
                  <w:jc w:val="center"/>
                  <w:rPr>
                    <w:b/>
                    <w:bCs/>
                  </w:rPr>
                </w:pPr>
                <w:r w:rsidRPr="00432EB6">
                  <w:rPr>
                    <w:b/>
                    <w:bCs/>
                  </w:rPr>
                  <w:t>Комментарий</w:t>
                </w:r>
              </w:p>
            </w:tc>
          </w:tr>
        </w:tbl>
        <w:p w:rsidR="00C5707F" w:rsidRPr="00C5707F" w:rsidRDefault="00C5707F" w:rsidP="00C5707F">
          <w:pPr>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
          <w:bookmarkStart w:id="0" w:name="_GoBack"/>
          <w:bookmarkEnd w:id="0"/>
        </w:p>
        <w:p w:rsidR="00C5707F" w:rsidRPr="00C5707F" w:rsidRDefault="00C5707F" w:rsidP="00C5707F">
          <w:pPr>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
        </w:p>
        <w:p w:rsidR="00AB36EE" w:rsidRPr="00C5707F" w:rsidRDefault="00C5707F" w:rsidP="00C5707F">
          <w:pPr>
            <w:tabs>
              <w:tab w:val="left" w:pos="1457"/>
              <w:tab w:val="left" w:pos="2847"/>
              <w:tab w:val="left" w:pos="4521"/>
            </w:tabs>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
          </w:pPr>
          <w:r>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:tab/>
          </w:r>
          <w:r>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:tab/>
          </w:r>
          <w:r>
            <w:rPr>
              <w:rFonts w:asciiTheme="minorHAnsi" w:hAnsiTheme="minorHAnsi" w:cstheme="minorHAnsi"/>
              <w:sz w:val="28"/>
              <w:szCs w:val="28"/>
            </w:rPr>
            <w:tab/>
          </w:r>
        </w:p>
        </xsl:for-each>
        <w:sectPr w:rsidR="00AB36EE" w:rsidRPr="00C5707F" w:rsidSect="00C5707F">
          <w:footerReference w:type="default" r:id="rId7"/>
          <w:headerReference w:type="first" r:id="rId8"/>
          <w:pgSz w:w="16838" w:h="11906" w:orient="landscape"/>
          <w:pgMar w:top="709" w:right="1134" w:bottom="567" w:left="709" w:header="568" w:footer="0" w:gutter="0"/>
          <w:cols w:space="708"/>
          <w:titlePg/>
          <w:docGrid w:linePitch="360"/>
        </w:sectPr>
      </w:body>
    </w:document>
  </xsl:template>
</xsl:stylesheet>